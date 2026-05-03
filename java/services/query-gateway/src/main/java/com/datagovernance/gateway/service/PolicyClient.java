package com.datagovernance.gateway.service;

import com.datagovernance.gateway.model.PolicyDto;
import lombok.RequiredArgsConstructor;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.core.ParameterizedTypeReference;
import org.springframework.stereotype.Service;
import org.springframework.web.reactive.function.client.WebClient;

import java.util.Collections;
import java.util.List;

@Service
@RequiredArgsConstructor
public class PolicyClient {

    @Value("${policy.service.url:http://localhost:8082}")
    private String policyServiceUrl;

    private final WebClient.Builder webClientBuilder;

    public List<PolicyDto> getPoliciesForRole(String role) {
        try {
            return webClientBuilder.baseUrl(policyServiceUrl)
                    .build()
                    .get()
                    .uri("/policies?role=" + role)
                    .retrieve()
                    .bodyToMono(new ParameterizedTypeReference<List<PolicyDto>>() {})
                    .block();
        } catch (Exception e) {
            return Collections.emptyList();
        }
    }
}
