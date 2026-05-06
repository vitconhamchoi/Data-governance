package com.datagovernance.aiclassifier.service;

import com.datagovernance.aiclassifier.model.ColumnClassification;
import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import lombok.RequiredArgsConstructor;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.http.HttpHeaders;
import org.springframework.http.MediaType;
import org.springframework.stereotype.Component;
import org.springframework.web.reactive.function.client.WebClient;

import java.util.List;
import java.util.Map;

@Component
@RequiredArgsConstructor
public class OpenAiPiiClassifierClient {

    private final WebClient.Builder webClientBuilder;
    private final ObjectMapper objectMapper;

    @Value("${ai.llm.base-url:https://api.openai.com}")
    private String baseUrl;

    @Value("${ai.llm.api-key:}")
    private String apiKey;

    @Value("${ai.llm.model:gpt-4o-mini}")
    private String model;

    public ColumnClassification classify(String column, String sample) {
        if (apiKey == null || apiKey.isBlank()) {
            throw new IllegalStateException("AI LLM API key is required");
        }

        String prompt = "Classify this value for data governance. " +
                "Return strict JSON {\"type\":\"PII.email|PII.phone|PII.name|PII.sensitive_text|NON_PII\",\"confidence\":0..1}. " +
                "Input: Column: " + column + " Sample: \"" + sample + "\"";

        Map<String, Object> payload = Map.of(
                "model", model,
                "response_format", Map.of("type", "json_object"),
                "messages", List.of(
                        Map.of("role", "system", "content", "You are a strict PII classifier."),
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
            JsonNode parsed = objectMapper.readTree(content);
            String type = parsed.path("type").asText("NON_PII");
            double confidence = parsed.path("confidence").asDouble(0.0);
            return ColumnClassification.builder()
                    .column(column)
                    .sampleValue(sample)
                    .type(type)
                    .confidence(confidence)
                    .source("llm")
                    .build();
        } catch (Exception ex) {
            throw new IllegalStateException("Failed to parse classifier response", ex);
        }
    }
}
