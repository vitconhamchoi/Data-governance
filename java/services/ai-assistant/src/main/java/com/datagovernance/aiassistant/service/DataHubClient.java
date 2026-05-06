package com.datagovernance.aiassistant.service;

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
public class DataHubClient {

    private final WebClient.Builder webClientBuilder;

    @Value("${datahub.graphql.url:http://localhost:9002/api/graphql}")
    private String graphqlUrl;

    @Value("${datahub.token:}")
    private String token;

    public String loadDatasetMetadata(String datasetUrnOrName) {
        String query = "query dataset($urn: String!) { dataset(urn: $urn) { urn properties { name description customProperties } tags { tags { tag { urn } } } } }";
        Map<String, Object> body = Map.of("query", query, "variables", Map.of("urn", datasetUrnOrName));

        try {
            WebClient.RequestBodySpec req = webClientBuilder.build().post().uri(graphqlUrl)
                    .contentType(MediaType.APPLICATION_JSON);
            if (token != null && !token.isBlank()) {
                req = req.header(HttpHeaders.AUTHORIZATION, "Bearer " + token);
            }
            JsonNode response = req.bodyValue(body).retrieve().bodyToMono(JsonNode.class).block();
            JsonNode dataset = response.path("data").path("dataset");
            if (dataset.isMissingNode() || dataset.isNull()) {
                return "{}";
            }
            return dataset.toString();
        } catch (Exception ex) {
            log.warn("DataHub metadata query failed: {}", ex.getMessage());
            return "{}";
        }
    }
}
