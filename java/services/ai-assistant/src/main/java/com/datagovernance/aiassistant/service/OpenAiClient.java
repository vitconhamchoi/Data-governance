package com.datagovernance.aiassistant.service;

import com.fasterxml.jackson.databind.JsonNode;
import lombok.RequiredArgsConstructor;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.http.HttpHeaders;
import org.springframework.http.MediaType;
import org.springframework.stereotype.Component;
import org.springframework.web.reactive.function.client.WebClient;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

@Component
@RequiredArgsConstructor
public class OpenAiClient {

    private final WebClient.Builder webClientBuilder;

    @Value("${ai.llm.base-url:https://api.openai.com}")
    private String baseUrl;

    @Value("${ai.llm.api-key:}")
    private String apiKey;

    @Value("${ai.llm.model:gpt-4o-mini}")
    private String model;

    @Value("${ai.embedding.model:text-embedding-3-small}")
    private String embeddingModel;

    public List<Double> embedding(String input) {
        requireKey();
        Map<String, Object> payload = Map.of("model", embeddingModel, "input", input);

        JsonNode response = webClientBuilder.baseUrl(baseUrl)
                .build()
                .post()
                .uri("/v1/embeddings")
                .header(HttpHeaders.AUTHORIZATION, "Bearer " + apiKey)
                .contentType(MediaType.APPLICATION_JSON)
                .bodyValue(payload)
                .retrieve()
                .bodyToMono(JsonNode.class)
                .block();

        List<Double> vector = new ArrayList<>();
        response.path("data").path(0).path("embedding").forEach(node -> vector.add(node.asDouble()));
        return vector;
    }

    public String chat(String prompt) {
        requireKey();
        Map<String, Object> payload = Map.of(
                "model", model,
                "messages", List.of(
                        Map.of("role", "system", "content", "You are a data governance AI copilot."),
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

        return response.path("choices").path(0).path("message").path("content").asText();
    }

    private void requireKey() {
        if (apiKey == null || apiKey.isBlank()) {
            throw new IllegalStateException("AI LLM API key is required");
        }
    }
}
