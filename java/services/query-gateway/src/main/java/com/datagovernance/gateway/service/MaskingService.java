package com.datagovernance.gateway.service;

import com.datagovernance.gateway.model.PolicyDto;
import org.springframework.stereotype.Service;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

@Service
public class MaskingService {

    public Object maskValue(String columnName, Object value, List<PolicyDto> policies) {
        if (value == null) return null;

        for (PolicyDto policy : policies) {
            if (policy.getColumnName().equalsIgnoreCase(columnName)
                    && "MASK".equalsIgnoreCase(policy.getRule())) {
                return applyMask(columnName, value.toString());
            }
        }
        return value;
    }

    private String applyMask(String columnName, String value) {
        String lowerColumn = columnName.toLowerCase();
        if (lowerColumn.contains("email")) {
            return maskEmail(value);
        } else if (lowerColumn.contains("phone")) {
            return maskPhone(value);
        }
        return "****";
    }

    // alice@example.com → a***@example.com
    private String maskEmail(String email) {
        int atIndex = email.indexOf('@');
        if (atIndex <= 0) return "***@***";
        String local = email.substring(0, atIndex);
        String domain = email.substring(atIndex);
        String maskedLocal = local.charAt(0) + "***";
        return maskedLocal + domain;
    }

    // 0901234567 → ****4567
    private String maskPhone(String phone) {
        if (phone.length() <= 4) return "****";
        String lastFour = phone.substring(phone.length() - 4);
        return "****" + lastFour;
    }

    public boolean shouldDeny(String columnName, List<PolicyDto> policies) {
        for (PolicyDto policy : policies) {
            if (policy.getColumnName().equalsIgnoreCase(columnName)
                    && "DENY".equalsIgnoreCase(policy.getRule())) {
                return true;
            }
        }
        return false;
    }

    public Map<String, Object> applyPolicies(Map<String, Object> row, List<PolicyDto> policies) {
        if (policies == null || policies.isEmpty()) return row;

        for (String col : new ArrayList<>(row.keySet())) {
            if (shouldDeny(col, policies)) {
                row.put(col, "[DENIED]");
            } else {
                row.put(col, maskValue(col, row.get(col), policies));
            }
        }
        return row;
    }
}
