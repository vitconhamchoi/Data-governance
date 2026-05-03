# .NET Implementation

This directory contains the .NET implementation of the Data Governance & AI Platform architecture.

## Overview

The .NET implementation provides a production-ready, enterprise-grade implementation of the platform architecture using modern .NET technologies (.NET 8.0+).

## Architecture

The implementation follows the Clean Architecture pattern with clear separation of concerns:

```
dotnet/
├── src/
│   ├── Services/                    # Microservices
│   │   ├── DataGovernance.API/      # REST/GraphQL API service
│   │   ├── DataGovernance.Pipeline/ # Data processing pipeline service
│   │   ├── DataGovernance.Agent/    # AI agent orchestration service
│   │   └── DataGovernance.Ingestion/# Data ingestion service
│   ├── Core/                        # Core business logic
│   │   ├── DataGovernance.Domain/   # Domain entities and interfaces
│   │   ├── DataGovernance.Application/ # Application services and use cases
│   │   └── DataGovernance.Contracts/ # Shared contracts and DTOs
│   ├── Infrastructure/              # Infrastructure implementations
│   │   ├── DataGovernance.Infrastructure.Data/ # Data access (EF Core, Dapper)
│   │   ├── DataGovernance.Infrastructure.Messaging/ # Event bus (Kafka, RabbitMQ)
│   │   ├── DataGovernance.Infrastructure.Cache/ # Caching (Redis)
│   │   ├── DataGovernance.Infrastructure.Storage/ # Object storage (S3, Azure Blob)
│   │   └── DataGovernance.Infrastructure.AI/ # AI services integration
│   └── Integrations/                # External integrations
│       ├── DataGovernance.Integrations.DataHub/ # DataHub metadata integration
│       ├── DataGovernance.Integrations.DataQuality/ # Data quality tools
│       ├── DataGovernance.Integrations.Lineage/ # Lineage tracking
│       └── DataGovernance.Integrations.IoT/ # IoT device connectivity
├── tests/                           # Test projects
│   ├── UnitTests/
│   ├── IntegrationTests/
│   └── E2ETests/
├── deployment/                      # Deployment configurations
│   ├── docker/                      # Docker files
│   ├── kubernetes/                  # K8s manifests
│   ├── terraform/                   # Infrastructure as Code
│   └── helm/                        # Helm charts
├── docs/                           # Documentation
│   ├── api/                        # API documentation
│   ├── architecture/               # Architecture decision records
│   └── guides/                     # Developer guides
└── samples/                        # Sample applications and demos

```

## Technology Stack

### Core Framework
- **.NET 8.0**: Latest LTS version with performance improvements
- **C# 12**: Latest language features

### API Layer
- **ASP.NET Core 8.0**: Web API framework
- **Minimal APIs**: Lightweight API endpoints
- **HotChocolate**: GraphQL server for .NET
- **Carter**: For organizing minimal API endpoints
- **FastEndpoints**: Alternative high-performance endpoint library

### Data Access
- **Entity Framework Core 8.0**: Primary ORM
- **Dapper**: High-performance micro-ORM for read-heavy operations
- **Npgsql**: PostgreSQL provider
- **StackExchange.Redis**: Redis client

### Messaging & Events
- **MassTransit**: Distributed application framework
- **Confluent.Kafka**: Kafka client for .NET
- **RabbitMQ.Client**: RabbitMQ client
- **Rebus**: Simple and lean service bus

### AI & ML
- **Microsoft.SemanticKernel**: LLM orchestration framework
- **Microsoft.ML**: Machine learning framework
- **LangChain.NET**: .NET port of LangChain
- **Pgvector.EntityFrameworkCore**: Vector similarity search

### Observability
- **OpenTelemetry**: Distributed tracing and metrics
- **Serilog**: Structured logging
- **Application Insights**: Azure monitoring (optional)
- **Prometheus.NET**: Metrics collection

### Testing
- **xUnit**: Primary testing framework
- **FluentAssertions**: Readable assertions
- **Moq**: Mocking framework
- **Testcontainers**: Integration testing with Docker
- **Bogus**: Test data generation

### DevOps & Deployment
- **Docker**: Containerization
- **Kubernetes**: Container orchestration
- **Helm**: Kubernetes package manager
- **Terraform**: Infrastructure as Code
- **GitHub Actions**: CI/CD pipelines

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [PostgreSQL](https://www.postgresql.org/) (or Docker container)
- [Redis](https://redis.io/) (or Docker container)
- [Kafka](https://kafka.apache.org/) (optional, or Docker container)

### Quick Start

1. **Clone the repository**
   ```bash
   git clone https://github.com/vitconhamchoi/Data-governance.git
   cd Data-governance/implementation/dotnet
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Start infrastructure services**
   ```bash
   cd deployment/docker
   docker-compose up -d
   ```

4. **Run database migrations**
   ```bash
   cd src/Services/DataGovernance.API
   dotnet ef database update
   ```

5. **Run the API service**
   ```bash
   cd src/Services/DataGovernance.API
   dotnet run
   ```

6. **Access the API**
   - REST API: https://localhost:5001/api
   - GraphQL: https://localhost:5001/graphql
   - Swagger UI: https://localhost:5001/swagger

### Running Tests

```bash
# Run all tests
dotnet test

# Run unit tests only
dotnet test --filter Category=Unit

# Run integration tests
dotnet test --filter Category=Integration

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverageReportFormat=opencover
```

## Project Structure

### Services Layer

#### API Service
- RESTful API with minimal APIs
- GraphQL endpoint with HotChocolate
- Authentication & Authorization (JWT, OAuth2)
- Rate limiting and throttling
- API versioning
- Request/response logging
- Health checks

#### Pipeline Service
- Stream processing with Kafka Streams
- Batch processing with background services
- Data transformation and enrichment
- Multi-zone lakehouse processing (Raw → Standardized → Curated → Trusted)
- Scheduled jobs with Quartz.NET

#### Agent Service
- AI agent orchestration with Semantic Kernel
- Tool harness for LLM interactions
- Session and run tracking
- Approval workflow engine
- Memory management
- RAG (Retrieval Augmented Generation) support

#### Ingestion Service
- MQTT broker integration
- HTTP/HTTPS ingestion endpoints
- Device authentication and registry
- Protocol translation
- Message validation and routing

### Core Layer

#### Domain
- Entity models
- Domain events
- Value objects
- Aggregates
- Domain services
- Repository interfaces

#### Application
- Use cases / application services
- CQRS command and query handlers
- DTOs and mappers
- Validation rules
- Business logic orchestration

#### Contracts
- Shared DTOs
- API request/response models
- Event schemas
- Interface contracts

### Infrastructure Layer

#### Data
- EF Core DbContext configurations
- Repository implementations
- Database migrations
- Query specifications
- Unit of Work pattern

#### Messaging
- Event bus abstraction and implementation
- Message serialization
- Outbox pattern for reliable messaging
- Event handlers

#### Cache
- Redis distributed cache
- In-memory cache
- Cache-aside pattern
- Cache invalidation strategies

#### Storage
- Object storage abstraction (S3, Azure Blob)
- File upload/download services
- Blob lifecycle management

#### AI
- LLM client wrappers
- Vector database integration
- Embedding generation
- Semantic search

### Integrations Layer

Each integration provides:
- Client libraries
- API wrappers
- Data synchronization
- Event adapters
- Health checks

## Configuration

Configuration is managed through:
- `appsettings.json`: Default settings
- `appsettings.{Environment}.json`: Environment-specific settings
- Environment variables: Production secrets
- Azure Key Vault: Production secrets (optional)
- User secrets: Development secrets

### Example Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=datagovernance;Username=postgres;Password=postgres",
    "Redis": "localhost:6379",
    "Kafka": "localhost:9092"
  },
  "Authentication": {
    "Jwt": {
      "Secret": "your-secret-key-min-32-chars",
      "Issuer": "DataGovernance",
      "Audience": "DataGovernance.API",
      "ExpirationMinutes": 60
    }
  },
  "AI": {
    "OpenAI": {
      "ApiKey": "sk-...",
      "Model": "gpt-4",
      "MaxTokens": 2000
    },
    "VectorDatabase": {
      "Provider": "Pgvector",
      "Dimensions": 1536
    }
  },
  "Observability": {
    "ApplicationInsights": {
      "InstrumentationKey": ""
    },
    "OpenTelemetry": {
      "ServiceName": "DataGovernance.API",
      "JaegerEndpoint": "http://localhost:14268"
    }
  }
}
```

## Development Guidelines

### Code Style
- Follow [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use EditorConfig for consistent formatting
- Enable nullable reference types
- Use latest C# language features appropriately

### Architecture Principles
- **Clean Architecture**: Dependency rule - inner layers don't depend on outer layers
- **SOLID Principles**: Single responsibility, Open/closed, Liskov substitution, Interface segregation, Dependency inversion
- **DRY**: Don't Repeat Yourself
- **YAGNI**: You Aren't Gonna Need It
- **KISS**: Keep It Simple, Stupid

### Testing Strategy
- **Unit Tests**: Test individual components in isolation
- **Integration Tests**: Test component interactions with infrastructure
- **E2E Tests**: Test complete user scenarios
- **Test Coverage**: Aim for >80% code coverage
- **TDD**: Consider test-driven development for complex logic

### Performance Considerations
- Use async/await for I/O operations
- Implement caching strategies
- Use efficient data structures
- Profile and optimize hot paths
- Consider memory allocation patterns
- Use object pooling where appropriate

### Security Best Practices
- Input validation and sanitization
- Output encoding
- SQL injection prevention (parameterized queries)
- XSS protection
- CSRF protection
- Secure password hashing (BCrypt, Argon2)
- Principle of least privilege
- Regular dependency updates

## Deployment

### Docker

Build and run with Docker:

```bash
# Build image
docker build -t datagovernance-api:latest -f deployment/docker/Dockerfile.api .

# Run container
docker run -p 5000:80 -e ASPNETCORE_ENVIRONMENT=Development datagovernance-api:latest
```

### Kubernetes

Deploy to Kubernetes:

```bash
# Apply manifests
kubectl apply -f deployment/kubernetes/

# Or use Helm
helm install datagovernance deployment/helm/datagovernance
```

### Terraform

Provision infrastructure:

```bash
cd deployment/terraform
terraform init
terraform plan
terraform apply
```

## Monitoring & Observability

### Health Checks

Health check endpoints:
- `/health`: Overall health
- `/health/ready`: Readiness probe
- `/health/live`: Liveness probe

### Metrics

Prometheus metrics available at `/metrics`:
- Request rate and duration
- Error rate
- Database connection pool
- Cache hit/miss ratio
- Custom business metrics

### Logging

Structured logging with Serilog:
- Console sink for development
- File sink for structured logs
- Elasticsearch sink for centralized logging
- Application Insights sink (optional)

### Tracing

Distributed tracing with OpenTelemetry:
- Automatic HTTP request tracing
- Database query tracing
- Custom activity tracking
- Jaeger/Zipkin integration

## API Documentation

### REST API

Interactive API documentation available at:
- Swagger UI: `/swagger`
- OpenAPI spec: `/swagger/v1/swagger.json`

### GraphQL

GraphQL playground available at:
- Banana Cake Pop: `/graphql`
- GraphQL schema: `/graphql/schema`

## Contributing

Please read the main repository [CONTRIBUTING.md](../../../CONTRIBUTING.md) for contribution guidelines.

## Version History

- **v1.0.0** (2026-05-03): Initial .NET implementation
  - Core API service
  - Pipeline service foundation
  - Basic integrations
  - Docker and Kubernetes support

## License

See main repository [LICENSE](../../../LICENSE) file.

## Support & Contact

For questions and support:
- GitHub Issues: [Create an issue](https://github.com/vitconhamchoi/Data-governance/issues)
- Documentation: [Architecture docs](../../README.md)

## Related Documentation

- [Main Architecture Documentation](../../README.md)
- [IoMT Platform Architecture](../../01_iomt_platform_technical_architecture.md)
- [Harness Engineering](../../02_harness_engineering.md)
