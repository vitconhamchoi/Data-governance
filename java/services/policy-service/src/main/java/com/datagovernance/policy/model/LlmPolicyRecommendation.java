package com.datagovernance.policy.model;

import lombok.Data;

@Data
public class LlmPolicyRecommendation {
    private String rule;
    private String reason;
}
