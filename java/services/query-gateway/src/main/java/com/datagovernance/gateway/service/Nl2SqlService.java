package com.datagovernance.gateway.service;

import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;

@Service
@RequiredArgsConstructor
public class Nl2SqlService {

    private final DataHubMetadataClient dataHubMetadataClient;
    private final OpenAiNl2SqlService openAiNl2SqlService;

    public String toSql(String question) {
        StringBuilder schema = new StringBuilder();
        for (String datasetUrnOrName : dataHubMetadataClient.extractTableHintsFromQuestion(question)) {
            schema.append("Dataset ").append(datasetUrnOrName).append(":\n")
                    .append(dataHubMetadataClient.getSchemaContext(datasetUrnOrName)).append("\n");
        }
        return openAiNl2SqlService.generateSql(question, schema.toString());
    }
}
