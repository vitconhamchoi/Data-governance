package com.datagovernance.aiassistant.service;

import com.datagovernance.aiassistant.model.IndexDatasetRequest;
import com.datagovernance.aiassistant.model.SemanticSearchResult;
import lombok.RequiredArgsConstructor;
import org.springframework.jdbc.core.JdbcTemplate;
import org.springframework.stereotype.Service;

import java.util.List;
import java.util.stream.Collectors;

@Service
@RequiredArgsConstructor
public class SemanticSearchService {

    private final JdbcTemplate jdbcTemplate;
    private final OpenAiClient openAiClient;
    private final DataHubClient dataHubClient;

    public void indexDataset(IndexDatasetRequest request) {
        String metadata = (request.getMetadataJson() == null || request.getMetadataJson().isBlank())
                ? dataHubClient.loadDatasetMetadata(request.getDataset())
                : request.getMetadataJson();

        String text = request.getDataset() + " " + (request.getDescription() == null ? "" : request.getDescription()) + " " + metadata;
        List<Double> embedding = openAiClient.embedding(text);

        String vectorLiteral = toPgVectorLiteral(embedding);
        jdbcTemplate.update(
                """
                INSERT INTO dataset_embeddings(dataset, metadata_json, embedding)
                VALUES (?, ?::jsonb, ?::vector)
                ON CONFLICT (dataset) DO UPDATE SET metadata_json = EXCLUDED.metadata_json, embedding = EXCLUDED.embedding, updated_at = now()
                """,
                request.getDataset(), metadata, vectorLiteral
        );
    }

    public List<SemanticSearchResult> search(String query, int topK) {
        List<Double> embedding = openAiClient.embedding(query);
        String vectorLiteral = toPgVectorLiteral(embedding);

        return jdbcTemplate.query(
                """
                WITH query_vector AS (SELECT ?::vector AS v)
                SELECT d.dataset, d.metadata_json::text, 1 - (d.embedding <=> q.v) AS similarity
                FROM dataset_embeddings d
                CROSS JOIN query_vector q
                ORDER BY d.embedding <=> q.v
                LIMIT ?
                """,
                ps -> {
                    ps.setString(1, vectorLiteral);
                    ps.setInt(2, Math.max(1, topK));
                },
                (rs, rowNum) -> SemanticSearchResult.builder()
                        .dataset(rs.getString("dataset"))
                        .metadataJson(rs.getString("metadata_json"))
                        .similarity(rs.getDouble("similarity"))
                        .build()
        );
    }

    public String answerWithContext(String question) {
        List<SemanticSearchResult> context = search(question, 5);
        String prompt = "Answer the governance question using context only where possible. " +
                "Question: " + question + "\nContext:\n" +
                context.stream()
                        .map(c -> c.getDataset() + ": " + c.getMetadataJson())
                        .collect(Collectors.joining("\n"));

        return openAiClient.chat(prompt);
    }

    private String toPgVectorLiteral(List<Double> vector) {
        return "[" + vector.stream().map(String::valueOf).collect(Collectors.joining(",")) + "]";
    }
}
