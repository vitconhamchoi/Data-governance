package com.datagovernance.aiclassifier.service;

import com.datagovernance.aiclassifier.model.ColumnClassification;
import org.springframework.stereotype.Component;

import java.util.Optional;
import java.util.regex.Pattern;

@Component
public class PiiRegexClassifier {

    private static final Pattern EMAIL = Pattern.compile("^[A-Za-z0-9+_.-]+@[A-Za-z0-9.-]+$");
    private static final Pattern PHONE = Pattern.compile("^(\\+?\\d{1,3})?[ -.]?\\d{9,11}$");
    private static final Pattern NAME = Pattern.compile("^[A-Z][a-z]+(\\s[A-Z][a-z]+)+$");

    public Optional<ColumnClassification> classify(String column, String sample) {
        if (sample == null || sample.isBlank()) {
            return Optional.empty();
        }
        if (EMAIL.matcher(sample).matches()) {
            return Optional.of(ColumnClassification.builder().column(column).sampleValue(sample).type("PII.email").confidence(0.99).source("regex").build());
        }
        if (PHONE.matcher(sample).matches()) {
            return Optional.of(ColumnClassification.builder().column(column).sampleValue(sample).type("PII.phone").confidence(0.98).source("regex").build());
        }
        if (NAME.matcher(sample).matches()) {
            return Optional.of(ColumnClassification.builder().column(column).sampleValue(sample).type("PII.name").confidence(0.9).source("regex").build());
        }
        return Optional.empty();
    }
}
