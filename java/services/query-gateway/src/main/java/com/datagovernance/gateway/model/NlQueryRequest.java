package com.datagovernance.gateway.model;

import jakarta.validation.constraints.NotBlank;
import lombok.Data;

@Data
public class NlQueryRequest {
    @NotBlank
    private String question;

    @NotBlank
    private String role;
}
