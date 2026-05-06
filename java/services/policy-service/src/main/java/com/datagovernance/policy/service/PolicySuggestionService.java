package com.datagovernance.policy.service;

import com.datagovernance.policy.entity.Policy;
import com.datagovernance.policy.entity.PolicySuggestion;
import com.datagovernance.policy.model.LlmPolicyRecommendation;
import com.datagovernance.policy.model.PolicyRecommendationRequest;
import com.datagovernance.policy.repository.PolicyRepository;
import com.datagovernance.policy.repository.PolicySuggestionRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.time.LocalDateTime;
import java.util.List;

@Service
@RequiredArgsConstructor
public class PolicySuggestionService {

    private final PolicySuggestionRepository suggestionRepository;
    private final PolicyRepository policyRepository;
    private final OpenAiPolicyClient openAiPolicyClient;

    @Transactional
    public PolicySuggestion recommendAndStore(PolicyRecommendationRequest request) {
        LlmPolicyRecommendation recommendation = openAiPolicyClient.recommend(
                request.getDataset(), request.getColumnName(), request.getTag());

        PolicySuggestion suggestion = PolicySuggestion.builder()
                .dataset(request.getDataset())
                .columnName(request.getColumnName())
                .tag(request.getTag())
                .suggestedRule(recommendation.getRule())
                .reason(recommendation.getReason())
                .status("PENDING")
                .build();

        return suggestionRepository.save(suggestion);
    }

    public List<PolicySuggestion> getAll(String status) {
        if (status == null || status.isBlank()) {
            return suggestionRepository.findAll();
        }
        return suggestionRepository.findByStatus(status.toUpperCase());
    }

    @Transactional
    public PolicySuggestion approve(Long id, String approvedBy, String role) {
        PolicySuggestion suggestion = suggestionRepository.findById(id)
                .orElseThrow(() -> new IllegalArgumentException("Suggestion not found"));

        suggestion.setStatus("APPROVED");
        suggestion.setApprovedBy(approvedBy);
        suggestion.setApprovedAt(LocalDateTime.now());

        Policy policy = Policy.builder()
                .dataset(suggestion.getDataset())
                .columnName(suggestion.getColumnName())
                .rule(suggestion.getSuggestedRule())
                .role(role)
                .build();
        policyRepository.save(policy);

        return suggestionRepository.save(suggestion);
    }
}
