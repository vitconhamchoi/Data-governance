package com.datagovernance.policy.repository;

import com.datagovernance.policy.entity.Policy;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import java.util.List;

@Repository
public interface PolicyRepository extends JpaRepository<Policy, Long> {
    List<Policy> findByDatasetAndRole(String dataset, String role);
    List<Policy> findByRole(String role);
    List<Policy> findByDataset(String dataset);
}
