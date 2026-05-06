package com.datagovernance.aiclassifier.service;

import lombok.RequiredArgsConstructor;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;

import java.sql.*;
import java.util.*;

@Service
@RequiredArgsConstructor
public class TrinoDatasetSampler {

    @Value("${trino.jdbc.url:jdbc:trino://localhost:8080/postgresql/public}")
    private String trinoJdbcUrl;

    @Value("${trino.jdbc.user:admin}")
    private String trinoUser;

    public Map<String, String> sampleOneValuePerColumn(String dataset, int limit) {
        String sql = "SELECT * FROM " + dataset + " LIMIT " + Math.max(1, limit);
        Properties props = new Properties();
        props.setProperty("user", trinoUser);

        Map<String, String> samples = new LinkedHashMap<>();

        try (Connection conn = DriverManager.getConnection(trinoJdbcUrl, props);
             Statement stmt = conn.createStatement();
             ResultSet rs = stmt.executeQuery(sql)) {
            ResultSetMetaData meta = rs.getMetaData();
            int colCount = meta.getColumnCount();
            for (int i = 1; i <= colCount; i++) {
                samples.put(meta.getColumnName(i), "");
            }

            while (rs.next()) {
                for (int i = 1; i <= colCount; i++) {
                    String col = meta.getColumnName(i);
                    if (samples.get(col).isBlank()) {
                        Object value = rs.getObject(i);
                        samples.put(col, value == null ? "" : value.toString());
                    }
                }
            }
        } catch (Exception ex) {
            throw new IllegalStateException("Failed to sample dataset from Trino: " + ex.getMessage(), ex);
        }

        return samples;
    }
}
