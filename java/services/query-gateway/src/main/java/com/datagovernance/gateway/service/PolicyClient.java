package com.datagovernance.gateway.service;

import com.datagovernance.gateway.model.PolicyDto;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.core.ParameterizedTypeReference;
import org.springframework.stereotype.Service;
import org.springframework.web.reactive.function.client.WebClient;
import org.springframework.web.util.UriComponentsBuilder;

import java.util.Collections;
import java.util.List;

@Service
@RequiredArgsConstructor
@Slf4j
public class PolicyClient {

    @Value("${policy.service.url:http://localhost:8082}")
    private String policyServiceUrl;

    private final WebClient.Builder webClientBuilder;

    public List<PolicyDto> getPoliciesForRole(String role) {
        try {
            String uri = UriComponentsBuilder.fromPath("/policies")
                    .queryParam("role", role)
                    .build()
                    .toUriString();
            return webClientBuilder.baseUrl(policyServiceUrl)
                    .build()
                    .get()
                    .uri(uri)
                    .retrieve()
                    .bodyToMono(new ParameterizedTypeReference<List<PolicyDto>>() {})
                    .block();
        } catch (Exception e) {
            log.warn("Failed to retrieve policies for role '{}': {}", role, e.getMessage());
            return Collections.emptyList();
        }
    }
}
