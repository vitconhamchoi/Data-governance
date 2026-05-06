package com.datagovernance.policy.entity;

import jakarta.persistence.*;
import jakarta.validation.constraints.NotBlank;
import lombok.*;

import java.time.LocalDateTime;

@Entity
@Table(name = "policy_suggestions")
@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class PolicySuggestion {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @NotBlank
    @Column(nullable = false)
    private String dataset;

    @NotBlank
    @Column(name = "column_name", nullable = false)
    private String columnName;

    @NotBlank
    @Column(nullable = false)
    private String tag;

    @NotBlank
    @Column(name = "suggested_rule", nullable = false)
    private String suggestedRule;

    @NotBlank
    @Column(nullable = false, length = 2000)
    private String reason;

    @Column(nullable = false)
    @Builder.Default
    private String status = "PENDING";

    @Column(name = "approved_by")
    private String approvedBy;

    @Column(name = "created_at")
    private LocalDateTime createdAt;

    @Column(name = "approved_at")
    private LocalDateTime approvedAt;

    @PrePersist
    protected void onCreate() {
        this.createdAt = LocalDateTime.now();
    }
}
