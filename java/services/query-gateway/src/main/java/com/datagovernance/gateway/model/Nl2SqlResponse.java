package com.datagovernance.gateway.model;

import lombok.Builder;
import lombok.Data;

@Data
@Builder
public class Nl2SqlResponse {
    private String question;
    private String sql;
}
