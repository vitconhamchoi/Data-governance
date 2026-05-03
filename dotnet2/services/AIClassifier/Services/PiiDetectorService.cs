using System.Text.RegularExpressions;
using AIClassifier.Models;

namespace AIClassifier.Services
{
    public interface IPiiDetectorService
    {
        ClassifyResult? DetectWithRegex(string columnName, IEnumerable<string> sampleValues);
    }

    public class PiiDetectorService : IPiiDetectorService
    {
        private static readonly Regex EmailRegex = new(
            @"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled);

        private static readonly Regex PhoneRegex = new(
            @"^[\+]?[\s\-\.]?[\(]?[0-9]{1,4}[\)]?[\s\-\.]?[0-9]{3}[\s\-\.]?[0-9]{4,6}$",
            RegexOptions.Compiled);

        private static readonly Regex SsnRegex = new(
            @"^\d{3}-\d{2}-\d{4}$",
            RegexOptions.Compiled);

        private static readonly Regex CreditCardRegex = new(
            @"^\d{4}[\s\-]?\d{4}[\s\-]?\d{4}[\s\-]?\d{4}$",
            RegexOptions.Compiled);

        private static readonly HashSet<string> EmailColumns = new(StringComparer.OrdinalIgnoreCase)
        {
            "email", "email_address", "emailaddress", "mail", "e_mail", "user_email", "customer_email"
        };

        private static readonly HashSet<string> PhoneColumns = new(StringComparer.OrdinalIgnoreCase)
        {
            "phone", "phone_number", "phonenumber", "mobile", "tel", "telephone",
            "cell", "contact_number", "fax", "mobile_number"
        };

        private static readonly HashSet<string> NameColumns = new(StringComparer.OrdinalIgnoreCase)
        {
            "name", "full_name", "fullname", "first_name", "last_name", "firstname",
            "lastname", "customer_name", "given_name", "surname", "display_name"
        };

        private static readonly HashSet<string> SensitiveColumns = new(StringComparer.OrdinalIgnoreCase)
        {
            "ssn", "social_security", "credit_card", "card_number", "password",
            "secret", "token", "dob", "date_of_birth", "birthdate", "birth_date",
            "salary", "income", "passport", "passport_number", "national_id",
            "driver_license", "bank_account", "routing_number", "pin"
        };

        public ClassifyResult? DetectWithRegex(string columnName, IEnumerable<string> sampleValues)
        {
            var values = sampleValues.Where(v => !string.IsNullOrWhiteSpace(v)).ToList();

            // Column-name-based detection (high confidence)
            if (EmailColumns.Contains(columnName))
            {
                double conf = values.Count > 0
                    ? Math.Max(0.80, (double)values.Count(v => EmailRegex.IsMatch(v)) / values.Count)
                    : 0.80;
                return Build(columnName, "PII.email", conf, "regex");
            }

            if (PhoneColumns.Contains(columnName))
            {
                double conf = values.Count > 0
                    ? Math.Max(0.75, (double)values.Count(v => IsPhone(v)) / values.Count)
                    : 0.75;
                return Build(columnName, "PII.phone", conf, "regex");
            }

            if (NameColumns.Contains(columnName))
                return Build(columnName, "PII.name", 0.85, "regex");

            if (SensitiveColumns.Contains(columnName))
                return Build(columnName, "sensitive", 0.90, "regex");

            // Value-pattern detection for unknown column names
            if (values.Count > 0)
            {
                double emailRatio = (double)values.Count(v => EmailRegex.IsMatch(v)) / values.Count;
                if (emailRatio >= 0.7)
                    return Build(columnName, "PII.email", emailRatio, "regex");

                double phoneRatio = (double)values.Count(v => IsPhone(v)) / values.Count;
                if (phoneRatio >= 0.6)
                    return Build(columnName, "PII.phone", phoneRatio, "regex");

                double ssnRatio = (double)values.Count(v => SsnRegex.IsMatch(v)) / values.Count;
                if (ssnRatio >= 0.5)
                    return Build(columnName, "sensitive.ssn", ssnRatio, "regex");

                double ccRatio = (double)values.Count(v => CreditCardRegex.IsMatch(v)) / values.Count;
                if (ccRatio >= 0.5)
                    return Build(columnName, "sensitive.credit_card", ccRatio, "regex");
            }

            return null; // No match — caller should fall through to LLM
        }

        private static bool IsPhone(string value)
        {
            var normalized = Regex.Replace(value, @"[\s\-\.\(\)\+]", "");
            return normalized.Length >= 7 && normalized.Length <= 15 && normalized.All(char.IsDigit);
        }

        private static ClassifyResult Build(string col, string type, double confidence, string method) =>
            new() { ColumnName = col, Type = type, Confidence = Math.Round(confidence, 2), DetectionMethod = method };
    }
}
