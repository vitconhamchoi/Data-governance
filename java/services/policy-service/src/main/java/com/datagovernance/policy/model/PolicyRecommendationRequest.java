package com.datagovernance.policy.model;

import jakarta.validation.constraints.NotBlank;
import lombok.Data;

@Data
public class PolicyRecommendationRequest {
    @NotBlank
    private String dataset;
    @NotBlank
    private String columnName;
    @NotBlank
    private String tag;
}
