package com.datagovernance.aiclassifier.service;

import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.http.MediaType;
import org.springframework.stereotype.Component;
import org.springframework.web.reactive.function.client.WebClient;

import java.util.Map;

@Component
@RequiredArgsConstructor
@Slf4j
public class PolicySuggestionClient {

    private final WebClient.Builder webClientBuilder;

    @Value("${policy.service.url:http://localhost:8082}")
    private String policyServiceUrl;

    public void requestRecommendation(String dataset, String column, String tag) {
        try {
            webClientBuilder.baseUrl(policyServiceUrl)
                    .build()
                    .post()
                    .uri("/policy-suggestions/recommend")
                    .contentType(MediaType.APPLICATION_JSON)
                    .bodyValue(Map.of("dataset", dataset, "columnName", column, "tag", tag))
                    .retrieve()
                    .toBodilessEntity()
                    .block();
        } catch (Exception ex) {
            log.warn("Failed to request policy recommendation for {}.{}: {}", dataset, column, ex.getMessage());
        }
    }
}
