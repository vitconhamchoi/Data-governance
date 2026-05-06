package com.datagovernance.aiclassifier.model;

import jakarta.validation.constraints.NotBlank;
import lombok.Data;

@Data
public class ClassifyDatasetRequest {
    @NotBlank
    private String dataset;

    private String datasetUrn;

    private int sampleLimit = 20;
}
