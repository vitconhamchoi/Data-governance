package com.datagovernance.policy.model;

import jakarta.validation.constraints.NotBlank;
import lombok.Data;

@Data
public class ApproveSuggestionRequest {
    @NotBlank
    private String approvedBy;
    @NotBlank
    private String role;
}
