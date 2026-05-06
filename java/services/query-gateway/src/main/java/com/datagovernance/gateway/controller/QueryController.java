package com.datagovernance.gateway.controller;

import com.datagovernance.gateway.model.Nl2SqlResponse;
import com.datagovernance.gateway.model.NlQueryRequest;
import com.datagovernance.gateway.model.QueryRequest;
import com.datagovernance.gateway.model.QueryResponse;
import com.datagovernance.gateway.service.Nl2SqlService;
import com.datagovernance.gateway.service.QueryExecutorService;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.sql.SQLException;
import java.util.Map;

@RestController
@RequestMapping("/query")
@RequiredArgsConstructor
@Slf4j
public class QueryController {

    private final QueryExecutorService queryExecutorService;
    private final Nl2SqlService nl2SqlService;

    @PostMapping
    public ResponseEntity<?> executeQuery(@Valid @RequestBody QueryRequest request) {
        try {
            QueryResponse response = queryExecutorService.executeQuery(request.getSql(), request.getRole());
            return ResponseEntity.ok(response);
        } catch (SQLException e) {
            log.error("Query execution failed: {}", e.getMessage());
            return ResponseEntity.badRequest().body(Map.of(
                "error", "Query execution failed",
                "message", "An error occurred while executing the query. Please check your SQL syntax."
            ));
        }
    }

    @PostMapping("/nl2sql")
    public ResponseEntity<?> nl2sql(@Valid @RequestBody NlQueryRequest request) {
        try {
            String sql = nl2SqlService.toSql(request.getQuestion());
            return ResponseEntity.ok(Nl2SqlResponse.builder().question(request.getQuestion()).sql(sql).build());
        } catch (Exception ex) {
            log.error("NL2SQL failed: {}", ex.getMessage());
            return ResponseEntity.badRequest().body(Map.of("error", "NL2SQL failed", "message", ex.getMessage()));
        }
    }

    @PostMapping("/nl")
    public ResponseEntity<?> executeNl(@Valid @RequestBody NlQueryRequest request) {
        try {
            String sql = nl2SqlService.toSql(request.getQuestion());
            QueryResponse response = queryExecutorService.executeQuery(sql, request.getRole());
            return ResponseEntity.ok(Map.of("sql", sql, "result", response));
        } catch (SQLException ex) {
            return ResponseEntity.badRequest().body(Map.of("error", "Query execution failed", "message", ex.getMessage()));
        } catch (Exception ex) {
            return ResponseEntity.badRequest().body(Map.of("error", "NL query failed", "message", ex.getMessage()));
        }
    }

    @GetMapping("/health")
    public ResponseEntity<Map<String, String>> health() {
        return ResponseEntity.ok(Map.of("status", "UP", "service", "query-gateway"));
    }
}
