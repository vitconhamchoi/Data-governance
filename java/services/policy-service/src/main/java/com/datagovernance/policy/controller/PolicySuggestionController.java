package com.datagovernance.policy.controller;

import com.datagovernance.policy.entity.PolicySuggestion;
import com.datagovernance.policy.model.ApproveSuggestionRequest;
import com.datagovernance.policy.model.PolicyRecommendationRequest;
import com.datagovernance.policy.service.PolicySuggestionService;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/policy-suggestions")
@RequiredArgsConstructor
public class PolicySuggestionController {

    private final PolicySuggestionService policySuggestionService;

    @PostMapping("/recommend")
    public ResponseEntity<PolicySuggestion> recommend(@Valid @RequestBody PolicyRecommendationRequest request) {
        return ResponseEntity.status(HttpStatus.CREATED).body(policySuggestionService.recommendAndStore(request));
    }

    @GetMapping
    public ResponseEntity<List<PolicySuggestion>> list(@RequestParam(required = false) String status) {
        return ResponseEntity.ok(policySuggestionService.getAll(status));
    }

    @PostMapping("/{id}/approve")
    public ResponseEntity<PolicySuggestion> approve(@PathVariable Long id, @Valid @RequestBody ApproveSuggestionRequest request) {
        return ResponseEntity.ok(policySuggestionService.approve(id, request.getApprovedBy(), request.getRole()));
    }
}
