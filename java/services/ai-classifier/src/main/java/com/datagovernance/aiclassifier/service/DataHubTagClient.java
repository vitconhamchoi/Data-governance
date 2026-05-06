package com.datagovernance.aiclassifier.service;

import com.fasterxml.jackson.databind.JsonNode;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.http.HttpHeaders;
import org.springframework.http.MediaType;
import org.springframework.stereotype.Component;
import org.springframework.web.reactive.function.client.WebClient;

import java.util.Map;

@Component
@RequiredArgsConstructor
@Slf4j
public class DataHubTagClient {

    private final WebClient.Builder webClientBuilder;

    @Value("${datahub.graphql.url:http://localhost:9002/api/graphql}")
    private String graphqlUrl;

    @Value("${datahub.token:}")
    private String token;

    public void addTag(String resourceUrn, String piiTag) {
        String mutation = "mutation addTag($input: AddTagInput!) { addTag(input: $input) }";
        Map<String, Object> variables = Map.of("input", Map.of(
                "tagUrn", "urn:li:tag:" + piiTag,
                "resourceUrn", resourceUrn
        ));

        try {
            WebClient.RequestBodySpec req = webClientBuilder.build().post().uri(graphqlUrl)
                    .contentType(MediaType.APPLICATION_JSON);
            if (token != null && !token.isBlank()) {
                req = req.header(HttpHeaders.AUTHORIZATION, "Bearer " + token);
            }

            JsonNode response = req.bodyValue(Map.of("query", mutation, "variables", variables))
                    .retrieve()
                    .bodyToMono(JsonNode.class)
                    .block();
            if (response == null || !response.path("errors").isMissingNode()) {
                log.warn("DataHub addTag returned errors for {} -> {}", resourceUrn, piiTag);
            }
        } catch (Exception ex) {
            log.warn("Failed tagging DataHub metadata: {}", ex.getMessage());
        }
    }
}
