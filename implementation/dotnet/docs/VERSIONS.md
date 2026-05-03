# Technology Versions and Dependencies

This document specifies the exact versions of all technologies, frameworks, and tools used in the .NET implementation of the Data Governance platform.

**Last Updated:** 2026-05-03
**Platform Version:** 1.0.0

## Core Framework

| Technology | Version | Release Date | Support Until | Notes |
|------------|---------|--------------|---------------|-------|
| .NET SDK | 8.0.x | Nov 2023 | Nov 2026 | LTS Release |
| C# Language | 12.0 | Nov 2023 | - | Latest features enabled |
| ASP.NET Core | 8.0.x | Nov 2023 | Nov 2026 | LTS Release |

## NuGet Packages

### Web API & HTTP

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.AspNetCore.OpenApi | 8.0.0 | OpenAPI support |
| Swashbuckle.AspNetCore | 6.5.0 | Swagger documentation |
| Carter | 7.2.0 | Minimal API organization |
| FastEndpoints | 5.21.0 | Alternative high-performance endpoints |

### Data Access

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.EntityFrameworkCore | 8.0.0 | ORM framework |
| Microsoft.EntityFrameworkCore.Design | 8.0.0 | Design-time tools |
| Npgsql.EntityFrameworkCore.PostgreSQL | 8.0.0 | PostgreSQL provider |
| Dapper | 2.1.28 | Micro-ORM for high performance |
| Microsoft.EntityFrameworkCore.Tools | 8.0.0 | Migration tools |

### Messaging & Events

| Package | Version | Purpose |
|---------|---------|---------|
| MassTransit | 8.1.3 | Message bus abstraction |
| MassTransit.Kafka | 8.1.3 | Kafka integration |
| MassTransit.RabbitMQ | 8.1.3 | RabbitMQ integration |
| Confluent.Kafka | 2.3.0 | Kafka client |
| RabbitMQ.Client | 6.6.0 | RabbitMQ client |

### Caching

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.Extensions.Caching.StackExchangeRedis | 8.0.0 | Redis distributed cache |
| StackExchange.Redis | 2.7.10 | Redis client |

### AI & ML

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.SemanticKernel | 1.3.0 | LLM orchestration |
| Microsoft.ML | 3.0.1 | Machine learning |
| Azure.AI.OpenAI | 1.0.0-beta.12 | OpenAI integration |
| Pgvector.EntityFrameworkCore | 0.1.0 | Vector similarity search |

### Observability

| Package | Version | Purpose |
|---------|---------|---------|
| OpenTelemetry | 1.7.0 | Telemetry SDK |
| OpenTelemetry.Exporter.Prometheus.AspNetCore | 1.7.0-rc.1 | Prometheus exporter |
| OpenTelemetry.Exporter.Jaeger | 1.5.1 | Jaeger exporter |
| OpenTelemetry.Instrumentation.AspNetCore | 1.7.0 | ASP.NET Core instrumentation |
| OpenTelemetry.Instrumentation.Http | 1.7.0 | HTTP instrumentation |
| Serilog.AspNetCore | 8.0.0 | Structured logging |
| Serilog.Sinks.Console | 5.0.1 | Console sink |
| Serilog.Sinks.File | 5.0.0 | File sink |
| Serilog.Sinks.Elasticsearch | 9.0.3 | Elasticsearch sink |

### Testing

| Package | Version | Purpose |
|---------|---------|---------|
| xUnit | 2.6.4 | Test framework |
| xunit.runner.visualstudio | 2.5.6 | Visual Studio test runner |
| FluentAssertions | 6.12.0 | Fluent assertions |
| Moq | 4.20.70 | Mocking framework |
| Testcontainers | 3.7.0 | Docker containers for tests |
| Bogus | 35.4.0 | Test data generation |
| Microsoft.AspNetCore.Mvc.Testing | 8.0.0 | Integration testing |

### Security

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.0 | JWT authentication |
| BCrypt.Net-Next | 4.0.3 | Password hashing |
| Microsoft.AspNetCore.DataProtection | 8.0.0 | Data protection |

### Utilities

| Package | Version | Purpose |
|---------|---------|---------|
| FluentValidation.AspNetCore | 11.3.0 | Model validation |
| AutoMapper | 12.0.1 | Object mapping |
| Polly | 8.2.1 | Resilience policies |
| Quartz | 3.8.0 | Job scheduling |

## Infrastructure Services

### Databases

| Service | Version | Docker Image | Purpose |
|---------|---------|--------------|---------|
| PostgreSQL | 16.x | postgres:16-alpine | Primary OLTP database |
| Redis | 7.x | redis:7-alpine | Distributed cache |
| Elasticsearch | 8.11.x | docker.elastic.co/elasticsearch/elasticsearch:8.11.0 | Search and logging |

### Message Brokers

| Service | Version | Docker Image | Purpose |
|---------|---------|--------------|---------|
| Apache Kafka | 3.6.x | confluentinc/cp-kafka:7.5.0 | Event streaming |
| Zookeeper | 3.8.x | confluentinc/cp-zookeeper:7.5.0 | Kafka coordination |

### Observability

| Service | Version | Docker Image | Purpose |
|---------|---------|--------------|---------|
| Prometheus | 2.48.x | prom/prometheus:v2.48.0 | Metrics collection |
| Grafana | 10.2.x | grafana/grafana:10.2.2 | Visualization |
| Jaeger | 1.51.x | jaegertracing/all-in-one:1.51 | Distributed tracing |

### Object Storage

| Service | Version | Docker Image | Purpose |
|---------|---------|--------------|---------|
| MinIO | Latest | minio/minio:RELEASE.2023-11-20T22-40-07Z | S3-compatible storage |

## Development Tools

| Tool | Version | Purpose |
|------|---------|---------|
| Docker Desktop | 24.0+ | Container runtime |
| Docker Compose | 2.23+ | Multi-container orchestration |
| Kubernetes | 1.28+ | Container orchestration |
| Helm | 3.13+ | Kubernetes package manager |
| kubectl | 1.28+ | Kubernetes CLI |

## CI/CD Tools

| Tool | Version | Purpose |
|------|---------|---------|
| GitHub Actions | N/A | CI/CD pipelines |
| Terraform | 1.6+ | Infrastructure as Code |
| Azure CLI | 2.54+ | Azure management (optional) |

## IDE & Editors

### Supported IDEs

| IDE | Version | Extensions/Plugins |
|-----|---------|-------------------|
| Visual Studio 2022 | 17.8+ | C# DevKit, .NET tools |
| Visual Studio Code | 1.85+ | C# DevKit, .NET Extension Pack |
| JetBrains Rider | 2023.3+ | Built-in .NET support |

### Recommended VS Code Extensions

- C# Dev Kit (microsoft.csdevkit)
- .NET Extension Pack (ms-dotnettools.vscode-dotnet-pack)
- C# Extensions (kreativ-software.csharpextensions)
- Docker (ms-azuretools.vscode-docker)
- Kubernetes (ms-kubernetes-tools.vscode-kubernetes-tools)
- REST Client (humao.rest-client)

## Operating System Support

| OS | Versions | Notes |
|----|----------|-------|
| Windows | 10, 11, Server 2019+ | Full support |
| macOS | 12+ (Monterey+) | Full support |
| Linux | Ubuntu 20.04+, RHEL 8+, Debian 11+ | Full support |

## Cloud Platform Support

| Platform | SDK Version | Services Used |
|----------|-------------|---------------|
| Azure | Latest | App Service, AKS, PostgreSQL, Redis Cache, Storage |
| AWS | Latest | EKS, RDS, ElastiCache, S3 |
| Google Cloud | Latest | GKE, Cloud SQL, Memorystore, Cloud Storage |

## Version Update Policy

### .NET Framework
- **LTS Releases Only:** We use Long-Term Support (LTS) releases of .NET
- **Update Cadence:** Major version updates reviewed every 3 years
- **Security Patches:** Applied within 1 week of release

### NuGet Packages
- **Major Updates:** Evaluated quarterly
- **Minor Updates:** Monthly review
- **Security Patches:** Applied immediately

### Infrastructure Services
- **Databases:** Minor version updates monthly, major versions annually
- **Message Brokers:** Updated quarterly
- **Observability Tools:** Updated bi-annually

## Compatibility Matrix

### .NET 8.0 Compatibility

| Component | Compatible | Notes |
|-----------|-----------|-------|
| Entity Framework Core 8.0 | ✅ Yes | Full support |
| ASP.NET Core 8.0 | ✅ Yes | Recommended |
| .NET 6.0 projects | ✅ Yes | Can reference, but upgrade recommended |
| .NET Framework | ⚠️ Limited | Use .NET Standard 2.0 for shared libraries |

### Database Compatibility

| Database | Minimum Version | Recommended Version |
|----------|----------------|---------------------|
| PostgreSQL | 12.0 | 16.0+ |
| Redis | 6.0 | 7.0+ |
| Elasticsearch | 7.17 | 8.11+ |

## Breaking Changes & Migration Guides

### From .NET 6 to .NET 8
- Minimal API improvements
- Enhanced performance
- New C# 12 features
- See [Migration Guide](./MIGRATION_NET6_TO_NET8.md)

### Entity Framework Core 8
- JSON columns support improved
- Performance improvements
- See [EF Core 8 Release Notes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-8.0/whatsnew)

## Security Updates

All dependencies are regularly scanned for vulnerabilities using:
- GitHub Dependabot
- .NET security advisories
- NuGet package vulnerability scanning

**Update Process:**
1. Weekly automated scans
2. Critical vulnerabilities patched within 24 hours
3. High severity within 1 week
4. Medium/Low severity in next release cycle

## References

- [.NET Release Notes](https://github.com/dotnet/core/tree/main/release-notes)
- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [NuGet Package Manager](https://www.nuget.org/)
