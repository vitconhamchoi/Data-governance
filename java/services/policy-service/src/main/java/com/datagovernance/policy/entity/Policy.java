package com.datagovernance.policy.entity;

import jakarta.persistence.*;
import jakarta.validation.constraints.NotBlank;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.time.LocalDateTime;

@Entity
@Table(name = "policies")
@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class Policy {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @NotBlank(message = "Dataset is required")
    @Column(nullable = false)
    private String dataset;

    @NotBlank(message = "Column name is required")
    @Column(name = "column_name", nullable = false)
    private String columnName;

    @NotBlank(message = "Rule is required")
    @Column(nullable = false)
    private String rule;

    @NotBlank(message = "Role is required")
    @Column(nullable = false)
    private String role;

    @Column(name = "created_at")
    private LocalDateTime createdAt;

    @PrePersist
    protected void onCreate() {
        this.createdAt = LocalDateTime.now();
    }
}
