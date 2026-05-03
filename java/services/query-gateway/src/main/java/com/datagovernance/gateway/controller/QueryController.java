package com.datagovernance.gateway.controller;

import com.datagovernance.gateway.model.QueryRequest;
import com.datagovernance.gateway.model.QueryResponse;
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

    @GetMapping("/health")
    public ResponseEntity<Map<String, String>> health() {
        return ResponseEntity.ok(Map.of("status", "UP", "service", "query-gateway"));
    }
}
