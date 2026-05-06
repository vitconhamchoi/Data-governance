package com.datagovernance.gateway.service;

import com.fasterxml.jackson.databind.JsonNode;
import lombok.RequiredArgsConstructor;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.http.HttpHeaders;
import org.springframework.http.MediaType;
import org.springframework.stereotype.Service;
import org.springframework.web.reactive.function.client.WebClient;

import java.util.List;
import java.util.Map;

@Service
@RequiredArgsConstructor
public class OpenAiNl2SqlService {

    private final WebClient.Builder webClientBuilder;

    @Value("${ai.llm.base-url:https://api.openai.com}")
    private String baseUrl;

    @Value("${ai.llm.api-key:}")
    private String apiKey;

    @Value("${ai.llm.model:gpt-4o-mini}")
    private String model;

    public String generateSql(String question, String schemaContext) {
        if (apiKey == null || apiKey.isBlank()) {
            throw new IllegalStateException("AI LLM API key is required");
        }

        String prompt = "Translate the user question to Trino SQL. Return JSON with key sql only. " +
                "Only output read-only SELECT/CTE.\nSchema:\n" + schemaContext + "\nQuestion: " + question;

        Map<String, Object> payload = Map.of(
                "model", model,
                "response_format", Map.of("type", "json_object"),
                "messages", List.of(
                        Map.of("role", "system", "content", "You are a SQL generation assistant for Trino."),
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
            JsonNode parsed = new com.fasterxml.jackson.databind.ObjectMapper().readTree(content);
            return parsed.path("sql").asText();
        } catch (Exception ex) {
            throw new IllegalStateException("Failed to parse NL2SQL response", ex);
        }
    }
}
