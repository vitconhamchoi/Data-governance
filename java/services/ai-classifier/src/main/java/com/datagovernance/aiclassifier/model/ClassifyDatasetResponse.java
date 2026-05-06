package com.datagovernance.aiclassifier.model;

import lombok.Builder;
import lombok.Data;

import java.util.List;

@Data
@Builder
public class ClassifyDatasetResponse {
    private String dataset;
    private List<ColumnClassification> findings;
    private int taggedCount;
}
