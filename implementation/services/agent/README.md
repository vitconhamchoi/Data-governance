# AI Agent Service

## Overview

Production-ready AI orchestration service implementing the AI Harness architecture with LLM agents, RAG, tool execution, and approval workflows.

## Architecture

```
User Request → Agent Orchestrator → [Planning] → [Tool Selection] → [Execution] → Response
                     ↓                  ↓              ↓              ↓
              Session Mgmt      LLM Service    Tool Harness    Approval Engine
                     ↓                  ↓              ↓              ↓
              State Machine      GPT-4/Claude   SQL/Search/API   Human Review
```

## Features

- **Multi-Agent Orchestration**: Coordinate multiple specialized agents
- **RAG (Retrieval Augmented Generation)**: Knowledge base integration
- **Tool Harness**: Safe execution of tools with approval workflows
- **Memory Management**: Conversation and context persistence
- **Observability**: Full tracing of agent runs and costs

## Technology Stack

- **Framework**: LangChain / LlamaIndex
- **LLM**: OpenAI GPT-4, Anthropic Claude
- **Vector DB**: Pinecone / Weaviate
- **Orchestration**: Custom state machine
- **Storage**: PostgreSQL, Redis

## Agent Types

### 1. Data Assistant Agent

Answers questions about data using SQL and metadata.

```python
# agents/data_assistant.py
from langchain.agents import create_sql_agent
from langchain.llms import OpenAI

class DataAssistantAgent:
    def __init__(self, db_connection):
        self.llm = OpenAI(temperature=0)
        self.agent = create_sql_agent(
            llm=self.llm,
            db=db_connection,
            verbose=True,
            handle_parsing_errors=True
        )

    def answer_question(self, question: str) -> str:
        # Agent automatically generates and executes SQL
        response = self.agent.run(question)
        return response

# Usage
agent = DataAssistantAgent(db)
answer = agent.answer_question("How many users signed up last month?")
```

### 2. RAG Document Agent

Answers questions using document knowledge base.

```python
# agents/rag_agent.py
from langchain.vectorstores import Pinecone
from langchain.embeddings import OpenAIEmbeddings
from langchain.chains import RetrievalQA

class RAGAgent:
    def __init__(self, index_name):
        self.embeddings = OpenAIEmbeddings()
        self.vectorstore = Pinecone.from_existing_index(
            index_name=index_name,
            embedding=self.embeddings
        )
        self.qa_chain = RetrievalQA.from_chain_type(
            llm=OpenAI(temperature=0),
            retriever=self.vectorstore.as_retriever(search_kwargs={"k": 5}),
            return_source_documents=True
        )

    def query(self, question: str):
        result = self.qa_chain({"query": question})
        return {
            "answer": result["result"],
            "sources": [doc.metadata for doc in result["source_documents"]]
        }
```

### 3. Workflow Agent

Executes multi-step workflows with approval gates.

```python
# agents/workflow_agent.py
from langchain.agents import Tool, AgentExecutor
from langchain.memory import ConversationBufferMemory

class WorkflowAgent:
    def __init__(self, tools, approval_callback):
        self.tools = tools
        self.approval_callback = approval_callback
        self.memory = ConversationBufferMemory(
            memory_key="chat_history",
            return_messages=True
        )

    async def execute_workflow(self, task: str):
        # Agent plans the workflow
        plan = await self.create_plan(task)

        # Check if approval needed
        if self.requires_approval(plan):
            approved = await self.approval_callback(plan)
            if not approved:
                return {"status": "denied", "plan": plan}

        # Execute plan step by step
        results = []
        for step in plan.steps:
            result = await self.execute_step(step)
            results.append(result)

        return {"status": "completed", "results": results}
```

## Tool Harness

Safe execution of tools with schema validation and approval.

```python
# tools/tool_harness.py
from typing import Callable, Dict, Any
from enum import Enum

class RiskLevel(Enum):
    LOW = "low"
    MEDIUM = "medium"
    HIGH = "high"

class Tool:
    def __init__(
        self,
        name: str,
        function: Callable,
        schema: Dict[str, Any],
        risk_level: RiskLevel,
        requires_approval: bool = False,
        timeout: int = 30
    ):
        self.name = name
        self.function = function
        self.schema = schema
        self.risk_level = risk_level
        self.requires_approval = requires_approval
        self.timeout = timeout

    async def execute(self, **kwargs):
        # Validate inputs against schema
        self.validate_inputs(kwargs)

        # Check approval if needed
        if self.requires_approval:
            await self.request_approval(kwargs)

        # Execute with timeout
        result = await asyncio.wait_for(
            self.function(**kwargs),
            timeout=self.timeout
        )

        return result

# Tool definitions
tools = [
    Tool(
        name="search_datasets",
        function=search_datasets,
        schema={"query": "string"},
        risk_level=RiskLevel.LOW
    ),
    Tool(
        name="execute_sql_query",
        function=execute_sql,
        schema={"query": "string", "database": "string"},
        risk_level=RiskLevel.MEDIUM,
        requires_approval=True
    ),
    Tool(
        name="delete_dataset",
        function=delete_dataset,
        schema={"dataset_id": "string"},
        risk_level=RiskLevel.HIGH,
        requires_approval=True
    )
]
```

## Orchestration State Machine

```python
# orchestration/state_machine.py
from enum import Enum

class RunState(Enum):
    PENDING = "pending"
    CLASSIFYING = "classifying"
    PLANNING = "planning"
    AWAITING_APPROVAL = "awaiting_approval"
    EXECUTING = "executing"
    WAITING_EXTERNAL = "waiting_external"
    SYNTHESIZING = "synthesizing"
    COMPLETED = "completed"
    FAILED = "failed"

class AgentRun:
    def __init__(self, session_id, task):
        self.id = uuid.uuid4()
        self.session_id = session_id
        self.task = task
        self.state = RunState.PENDING
        self.steps = []
        self.context = {}
        self.metrics = {
            "start_time": datetime.now(),
            "tokens_used": 0,
            "cost": 0.0
        }

    async def execute(self):
        try:
            self.state = RunState.CLASSIFYING
            task_type = await self.classify_task()

            self.state = RunState.PLANNING
            plan = await self.create_plan(task_type)

            if plan.requires_approval:
                self.state = RunState.AWAITING_APPROVAL
                await self.wait_for_approval()

            self.state = RunState.EXECUTING
            result = await self.execute_plan(plan)

            self.state = RunState.SYNTHESIZING
            response = await self.synthesize_response(result)

            self.state = RunState.COMPLETED
            self.metrics["end_time"] = datetime.now()

            return response

        except Exception as e:
            self.state = RunState.FAILED
            raise
```

## RAG Knowledge Base

```python
# rag/knowledge_base.py
from langchain.document_loaders import DirectoryLoader
from langchain.text_splitter import RecursiveCharacterTextSplitter
from langchain.vectorstores import Pinecone

class KnowledgeBase:
    def __init__(self, index_name):
        self.index_name = index_name
        self.embeddings = OpenAIEmbeddings()
        self.text_splitter = RecursiveCharacterTextSplitter(
            chunk_size=1000,
            chunk_overlap=200
        )

    def ingest_documents(self, directory_path):
        # Load documents
        loader = DirectoryLoader(directory_path, glob="**/*.md")
        documents = loader.load()

        # Split into chunks
        chunks = self.text_splitter.split_documents(documents)

        # Create embeddings and store
        Pinecone.from_documents(
            documents=chunks,
            embedding=self.embeddings,
            index_name=self.index_name
        )

    def search(self, query, k=5):
        vectorstore = Pinecone.from_existing_index(
            index_name=self.index_name,
            embedding=self.embeddings
        )
        return vectorstore.similarity_search(query, k=k)
```

## API Endpoints

```python
# api/agent_api.py
from fastapi import FastAPI, WebSocket

app = FastAPI()

@app.post("/api/v1/agent/sessions")
async def create_session(user_id: str):
    session = Session.create(user_id=user_id)
    return {"session_id": session.id}

@app.post("/api/v1/agent/runs")
async def create_run(session_id: str, task: str):
    run = AgentRun(session_id=session_id, task=task)
    result = await run.execute()
    return result

@app.websocket("/ws/agent/{session_id}")
async def agent_websocket(websocket: WebSocket, session_id: str):
    await websocket.accept()

    while True:
        message = await websocket.receive_text()

        # Create and execute agent run
        run = AgentRun(session_id=session_id, task=message)

        # Stream response
        async for chunk in run.stream_execute():
            await websocket.send_json(chunk)
```

## Directory Structure

```
agent/
├── agents/
│   ├── data_assistant.py
│   ├── rag_agent.py
│   └── workflow_agent.py
├── tools/
│   ├── tool_harness.py
│   ├── sql_tools.py
│   ├── search_tools.py
│   └── governance_tools.py
├── orchestration/
│   ├── state_machine.py
│   ├── session_manager.py
│   └── approval_engine.py
├── rag/
│   ├── knowledge_base.py
│   ├── document_processor.py
│   └── embeddings.py
├── api/
│   ├── agent_api.py
│   └── websocket_handler.py
├── config/
├── tests/
└── README.md
```

## Observability

```python
# Track all agent interactions
- LLM calls and token usage
- Tool executions
- Approval requests
- Cost per run
- Latency metrics
```

## Security

- **Input Validation**: All tool inputs validated
- **SQL Injection Prevention**: Parameterized queries only
- **Approval Gates**: High-risk actions require human approval
- **Audit Logging**: All actions logged for compliance
- **Rate Limiting**: Per-user limits on agent calls

## References

- [LangChain Documentation](https://python.langchain.com/)
- [LlamaIndex Documentation](https://docs.llamaindex.ai/)
