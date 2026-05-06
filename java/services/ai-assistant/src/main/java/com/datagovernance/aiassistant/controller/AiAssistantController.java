package com.datagovernance.aiassistant.controller;

import com.datagovernance.aiassistant.model.AskRequest;
import com.datagovernance.aiassistant.model.IndexDatasetRequest;
import com.datagovernance.aiassistant.model.SemanticSearchResult;
import com.datagovernance.aiassistant.service.SemanticSearchService;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.Map;

@RestController
@RequestMapping("/ai")
@RequiredArgsConstructor
public class AiAssistantController {

    private final SemanticSearchService semanticSearchService;

    @PostMapping("/index")
    public ResponseEntity<Map<String, String>> index(@Valid @RequestBody IndexDatasetRequest request) {
        semanticSearchService.indexDataset(request);
        return ResponseEntity.ok(Map.of("status", "indexed", "dataset", request.getDataset()));
    }

    @GetMapping("/search")
    public ResponseEntity<List<SemanticSearchResult>> search(@RequestParam("q") String query,
                                                             @RequestParam(defaultValue = "5") int topK) {
        return ResponseEntity.ok(semanticSearchService.search(query, topK));
    }

    @PostMapping("/ask")
    public ResponseEntity<Map<String, String>> ask(@Valid @RequestBody AskRequest request) {
        String answer = semanticSearchService.answerWithContext(request.getQuestion());
        return ResponseEntity.ok(Map.of("question", request.getQuestion(), "answer", answer));
    }

    @GetMapping("/health")
    public ResponseEntity<Map<String, String>> health() {
        return ResponseEntity.ok(Map.of("status", "UP", "service", "ai-assistant"));
    }
}
