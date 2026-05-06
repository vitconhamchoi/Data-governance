package com.datagovernance.aiclassifier.service;

import com.datagovernance.aiclassifier.model.ClassifyDatasetRequest;
import com.datagovernance.aiclassifier.model.ClassifyDatasetResponse;
import com.datagovernance.aiclassifier.model.ColumnClassification;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

@Service
@RequiredArgsConstructor
public class ClassifierOrchestratorService {

    private final TrinoDatasetSampler datasetSampler;
    private final PiiRegexClassifier regexClassifier;
    private final OpenAiPiiClassifierClient llmClassifierClient;
    private final DataHubTagClient dataHubTagClient;
    private final PolicySuggestionClient policySuggestionClient;

    public ClassifyDatasetResponse classify(ClassifyDatasetRequest request) {
        Map<String, String> samples = datasetSampler.sampleOneValuePerColumn(request.getDataset(), request.getSampleLimit());
        List<ColumnClassification> findings = new ArrayList<>();

        for (Map.Entry<String, String> entry : samples.entrySet()) {
            String column = entry.getKey();
            String sample = entry.getValue();

            ColumnClassification classification = regexClassifier.classify(column, sample)
                    .orElseGet(() -> llmClassifierClient.classify(column, sample));

            if (!"NON_PII".equalsIgnoreCase(classification.getType()) && classification.getType().startsWith("PII.")) {
                findings.add(classification);

                String datasetUrn = request.getDatasetUrn() != null && !request.getDatasetUrn().isBlank()
                        ? request.getDatasetUrn()
                        : "urn:li:dataset:(urn:li:dataPlatform:trino," + request.getDataset() + ",PROD)";
                dataHubTagClient.addTag(datasetUrn, classification.getType());
                policySuggestionClient.requestRecommendation(request.getDataset(), column, classification.getType());
            }
        }

        return ClassifyDatasetResponse.builder()
                .dataset(request.getDataset())
                .findings(findings)
                .taggedCount(findings.size())
                .build();
    }
}
