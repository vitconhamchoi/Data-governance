package com.datagovernance.policy.service;

import com.datagovernance.policy.entity.Policy;
import com.datagovernance.policy.repository.PolicyRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;

import java.util.List;

@Service
@RequiredArgsConstructor
public class PolicyService {

    private final PolicyRepository policyRepository;

    public List<Policy> getAllPolicies() {
        return policyRepository.findAll();
    }

    public Policy createPolicy(Policy policy) {
        return policyRepository.save(policy);
    }

    public void deletePolicy(Long id) {
        policyRepository.deleteById(id);
    }

    public List<Policy> getPoliciesByDatasetAndRole(String dataset, String role) {
        return policyRepository.findByDatasetAndRole(dataset, role);
    }

    public List<Policy> getPoliciesByRole(String role) {
        return policyRepository.findByRole(role);
    }
}
