package com.datagovernance.gateway.service;

import com.fasterxml.jackson.databind.JsonNode;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.http.HttpHeaders;
import org.springframework.http.MediaType;
import org.springframework.stereotype.Service;
import org.springframework.web.reactive.function.client.WebClient;

import java.util.List;
import java.util.Map;

@Service
@RequiredArgsConstructor
@Slf4j
public class DataHubMetadataClient {

    private final WebClient.Builder webClientBuilder;

    @Value("${datahub.graphql.url:http://localhost:9002/api/graphql}")
    private String graphqlUrl;

    @Value("${datahub.token:}")
    private String token;

    public String getSchemaContext(String dataset) {
        String query = "query dataset($urn: String!) { dataset(urn: $urn) { schemaMetadata { fields { fieldPath description type } } } }";
        Map<String, Object> body = Map.of(
                "query", query,
                "variables", Map.of("urn", dataset)
        );

        try {
            WebClient.RequestBodySpec req = webClientBuilder.build().post().uri(graphqlUrl)
                    .contentType(MediaType.APPLICATION_JSON);
            if (token != null && !token.isBlank()) {
                req = req.header(HttpHeaders.AUTHORIZATION, "Bearer " + token);
            }
            JsonNode response = req.bodyValue(body).retrieve().bodyToMono(JsonNode.class).block();
            JsonNode fields = response.path("data").path("dataset").path("schemaMetadata").path("fields");
            if (fields.isMissingNode() || !fields.isArray() || fields.isEmpty()) {
                return "No DataHub schema metadata found for dataset URN: " + dataset;
            }
            StringBuilder context = new StringBuilder();
            fields.forEach(field -> context
                    .append(field.path("fieldPath").asText())
                    .append(" (")
                    .append(field.path("type").asText("unknown"))
                    .append(") - ")
                    .append(field.path("description").asText(""))
                    .append("\n"));
            return context.toString();
        } catch (Exception ex) {
            log.warn("DataHub metadata fetch failed for {}: {}", dataset, ex.getMessage());
            return "DataHub schema unavailable for " + dataset;
        }
    }

    public List<String> extractTableHintsFromQuestion(String question) {
        String searchQuery = "query search($input: SearchInput!) { search(input: $input) { searchResults { entity { urn } } } }";
        Map<String, Object> input = Map.of(
                "type", "DATASET",
                "query", question,
                "start", 0,
                "count", 5
        );

        try {
            WebClient.RequestBodySpec req = webClientBuilder.build().post().uri(graphqlUrl)
                    .contentType(MediaType.APPLICATION_JSON);
            if (token != null && !token.isBlank()) {
                req = req.header(HttpHeaders.AUTHORIZATION, "Bearer " + token);
            }
            JsonNode response = req.bodyValue(Map.of("query", searchQuery, "variables", Map.of("input", input)))
                    .retrieve()
                    .bodyToMono(JsonNode.class)
                    .block();

            JsonNode results = response.path("data").path("search").path("searchResults");
            if (results.isArray() && !results.isEmpty()) {
                return results.findValuesAsText("urn");
            }
        } catch (Exception ex) {
            log.warn("DataHub search failed: {}", ex.getMessage());
        }

        return List.of("urn:li:dataset:(urn:li:dataPlatform:trino,orders,PROD)", "urn:li:dataset:(urn:li:dataPlatform:trino,users,PROD)");
    }
}
