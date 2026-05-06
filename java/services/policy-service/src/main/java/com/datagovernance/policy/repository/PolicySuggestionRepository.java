package com.datagovernance.policy.repository;

import com.datagovernance.policy.entity.PolicySuggestion;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;

public interface PolicySuggestionRepository extends JpaRepository<PolicySuggestion, Long> {
    List<PolicySuggestion> findByStatus(String status);
}
