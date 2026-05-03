using System.Text.RegularExpressions;

namespace QueryGateway.Services
{
    public interface IMaskingService
    {
        string MaskEmail(string email);
        string MaskPhone(string phone);
        object? ApplyMasking(object? value, string column, string rule);
    }

    public class MaskingService : IMaskingService
    {
        public string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains('@'))
                return email;

            var parts = email.Split('@');
            if (parts[0].Length <= 1)
                return email;

            return $"{parts[0][0]}***@{parts[1]}";
        }

        public string MaskPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return phone;

            // Remove non-numeric characters
            var digits = Regex.Replace(phone, @"[^\d]", "");

            if (digits.Length < 4)
                return "****";

            return "****" + digits.Substring(digits.Length - 4);
        }

        public object? ApplyMasking(object? value, string column, string rule)
        {
            if (value == null)
                return null;

            if (rule.ToLower() == "deny")
                return "[REDACTED]";

            if (rule.ToLower() == "mask")
            {
                var strValue = value.ToString();
                if (string.IsNullOrEmpty(strValue))
                    return value;

                // Detect type based on column name or value pattern
                if (column.ToLower().Contains("email"))
                    return MaskEmail(strValue);
                else if (column.ToLower().Contains("phone"))
                    return MaskPhone(strValue);
                else
                    return MaskGeneric(strValue);
            }

            return value;
        }

        private string MaskGeneric(string value)
        {
            if (value.Length <= 4)
                return new string('*', value.Length);

            return value.Substring(0, 2) + new string('*', value.Length - 4) + value.Substring(value.Length - 2);
        }
    }
}
