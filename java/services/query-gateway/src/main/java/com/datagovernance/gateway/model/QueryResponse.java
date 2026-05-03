package com.datagovernance.gateway.model;

import lombok.Builder;
import lombok.Data;

import java.util.List;
import java.util.Map;

@Data
@Builder
public class QueryResponse {
    private List<String> columns;
    private List<Map<String, Object>> rows;
    private int rowCount;
    private String role;
    private boolean masked;
}
