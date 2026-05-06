package com.datagovernance.policy.service;

import com.datagovernance.policy.model.LlmPolicyRecommendation;
import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.http.HttpHeaders;
import org.springframework.http.MediaType;
import org.springframework.stereotype.Component;
import org.springframework.web.reactive.function.client.WebClient;

import java.util.List;
import java.util.Map;

@Component
@RequiredArgsConstructor
@Slf4j
public class OpenAiPolicyClient {

    private final WebClient.Builder webClientBuilder;
    private final ObjectMapper objectMapper;

    @Value("${ai.llm.base-url:https://api.openai.com}")
    private String baseUrl;

    @Value("${ai.llm.api-key:}")
    private String apiKey;

    @Value("${ai.llm.model:gpt-4o-mini}")
    private String model;

    public LlmPolicyRecommendation recommend(String dataset, String columnName, String tag) {
        if (apiKey == null || apiKey.isBlank()) {
            throw new IllegalStateException("AI LLM API key is required");
        }

        String prompt = "You are a data governance policy assistant. " +
                "Given dataset, column and tag, output strict JSON with fields rule and reason. " +
                "Allowed rules: MASK, DENY, ALLOW. Prefer MASK for PII. " +
                "Input: dataset=" + dataset + ", column=" + columnName + ", tag=" + tag;

        Map<String, Object> payload = Map.of(
                "model", model,
                "response_format", Map.of("type", "json_object"),
                "messages", List.of(
                        Map.of("role", "system", "content", "Return only compact JSON."),
                        Map.of("role", "user", "content", prompt)
                )
        );

        JsonNode response = webClientBuilder.baseUrl(baseUrl)
                .build()
                .post()
                .uri("/v1/chat/completions")
                .header(HttpHeaders.AUTHORIZATION, "Bearer " + apiKey)
                .contentType(MediaType.APPLICATION_JSON)
                .bodyValue(payload)
                .retrieve()
                .bodyToMono(JsonNode.class)
                .block();

        try {
            String content = response.path("choices").path(0).path("message").path("content").asText();
            return objectMapper.readValue(content, LlmPolicyRecommendation.class);
        } catch (Exception ex) {
            log.error("Failed parsing LLM recommendation", ex);
            throw new IllegalStateException("Failed to parse LLM recommendation", ex);
        }
    }
}
