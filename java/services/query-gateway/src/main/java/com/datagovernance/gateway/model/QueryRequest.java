package com.datagovernance.gateway.model;

import jakarta.validation.constraints.NotBlank;
import lombok.Data;

@Data
public class QueryRequest {
    @NotBlank(message = "SQL query is required")
    private String sql;

    @NotBlank(message = "User role is required")
    private String role;
}
