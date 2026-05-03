package com.datagovernance.policy.controller;

import com.datagovernance.policy.entity.Policy;
import com.datagovernance.policy.service.PolicyService;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/policies")
@RequiredArgsConstructor
public class PolicyController {

    private final PolicyService policyService;

    @GetMapping
    public ResponseEntity<List<Policy>> getAllPolicies(
            @RequestParam(required = false) String dataset,
            @RequestParam(required = false) String role) {
        if (dataset != null && role != null) {
            return ResponseEntity.ok(policyService.getPoliciesByDatasetAndRole(dataset, role));
        } else if (role != null) {
            return ResponseEntity.ok(policyService.getPoliciesByRole(role));
        }
        return ResponseEntity.ok(policyService.getAllPolicies());
    }

    @PostMapping
    public ResponseEntity<Policy> createPolicy(@Valid @RequestBody Policy policy) {
        Policy created = policyService.createPolicy(policy);
        return ResponseEntity.status(HttpStatus.CREATED).body(created);
    }

    @DeleteMapping("/{id}")
    public ResponseEntity<Void> deletePolicy(@PathVariable Long id) {
        policyService.deletePolicy(id);
        return ResponseEntity.noContent().build();
    }
}
