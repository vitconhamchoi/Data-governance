package com.datagovernance.aiassistant.model;

import lombok.Builder;
import lombok.Data;

@Data
@Builder
public class SemanticSearchResult {
    private String dataset;
    private double similarity;
    private String metadataJson;
}
