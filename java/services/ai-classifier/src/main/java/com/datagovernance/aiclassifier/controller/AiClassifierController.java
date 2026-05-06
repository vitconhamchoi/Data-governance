package com.datagovernance.aiclassifier.controller;

import com.datagovernance.aiclassifier.model.ClassifyDatasetRequest;
import com.datagovernance.aiclassifier.model.ClassifyDatasetResponse;
import com.datagovernance.aiclassifier.service.ClassifierOrchestratorService;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.Map;

@RestController
@RequestMapping("/ai")
@RequiredArgsConstructor
public class AiClassifierController {

    private final ClassifierOrchestratorService classifierOrchestratorService;

    @PostMapping("/classify")
    public ResponseEntity<ClassifyDatasetResponse> classify(@Valid @RequestBody ClassifyDatasetRequest request) {
        return ResponseEntity.ok(classifierOrchestratorService.classify(request));
    }

    @GetMapping("/health")
    public ResponseEntity<Map<String, String>> health() {
        return ResponseEntity.ok(Map.of("status", "UP", "service", "ai-classifier"));
    }
}
