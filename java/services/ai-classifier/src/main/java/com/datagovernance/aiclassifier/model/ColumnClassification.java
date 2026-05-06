package com.datagovernance.aiclassifier.model;

import lombok.Builder;
import lombok.Data;

@Data
@Builder
public class ColumnClassification {
    private String column;
    private String type;
    private double confidence;
    private String sampleValue;
    private String source;
}
