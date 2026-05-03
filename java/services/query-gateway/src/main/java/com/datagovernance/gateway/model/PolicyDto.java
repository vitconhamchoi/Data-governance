package com.datagovernance.gateway.model;

import lombok.Data;

@Data
public class PolicyDto {
    private Long id;
    private String dataset;
    private String columnName;
    private String rule;
    private String role;
}
