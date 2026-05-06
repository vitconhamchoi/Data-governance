package com.datagovernance.aiassistant.model;

import jakarta.validation.constraints.NotBlank;
import lombok.Data;

@Data
public class IndexDatasetRequest {
    @NotBlank
    private String dataset;
    private String description;
    private String metadataJson;
}
