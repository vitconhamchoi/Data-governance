package com.datagovernance.gateway.service;

import com.datagovernance.gateway.model.PolicyDto;
import com.datagovernance.gateway.model.QueryResponse;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;

import java.sql.*;
import java.util.*;

@Service
@RequiredArgsConstructor
@Slf4j
public class QueryExecutorService {

    @Value("${trino.jdbc.url:jdbc:trino://localhost:8080/postgresql/public}")
    private String trinoJdbcUrl;

    @Value("${trino.jdbc.user:admin}")
    private String trinoUser;

    private final PolicyClient policyClient;
    private final MaskingService maskingService;

    public QueryResponse executeQuery(String sql, String role) throws SQLException {
        validateQuery(sql);

        List<PolicyDto> policies = policyClient.getPoliciesForRole(role);
        log.info("Executing query for role={} with {} applicable policies", role, policies.size());

        Properties props = new Properties();
        props.setProperty("user", trinoUser);

        List<String> columns = new ArrayList<>();
        List<Map<String, Object>> rows = new ArrayList<>();

        try (Connection conn = DriverManager.getConnection(trinoJdbcUrl, props);
             Statement stmt = conn.createStatement();
             ResultSet rs = stmt.executeQuery(sql)) {

            ResultSetMetaData meta = rs.getMetaData();
            int colCount = meta.getColumnCount();

            for (int i = 1; i <= colCount; i++) {
                columns.add(meta.getColumnName(i));
            }

            while (rs.next()) {
                Map<String, Object> row = new LinkedHashMap<>();
                for (int i = 1; i <= colCount; i++) {
                    String colName = meta.getColumnName(i);
                    Object value = rs.getObject(i);
                    row.put(colName, value);
                }
                // Apply masking policies at ResultSet level
                maskingService.applyPolicies(row, policies);
                rows.add(row);
            }
        }

        return QueryResponse.builder()
                .columns(columns)
                .rows(rows)
                .rowCount(rows.size())
                .role(role)
                .masked(!policies.isEmpty())
                .build();
    }

    /** Reject any statement that is not a read-only SELECT. */
    private void validateQuery(String sql) throws SQLException {
        if (sql == null || sql.isBlank()) {
            throw new SQLException("Query must not be empty");
        }
        String normalized = sql.stripLeading().toUpperCase();
        if (!normalized.startsWith("SELECT") && !normalized.startsWith("WITH")) {
            throw new SQLException("Only SELECT queries are permitted");
        }
    }
}
