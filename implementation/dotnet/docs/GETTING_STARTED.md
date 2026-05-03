# Data Governance .NET Implementation - Getting Started

This guide will help you get the .NET implementation of the Data Governance platform up and running on your local machine.

## Prerequisites

Before you begin, ensure you have the following installed:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for running infrastructure services)
- [Git](https://git-scm.com/) (for version control)
- A code editor (recommended: [Visual Studio Code](https://code.visualstudio.com/) or [JetBrains Rider](https://www.jetbrains.com/rider/))

### Optional Tools

- [Azure Data Studio](https://azure.microsoft.com/en-us/products/data-studio/) or [pgAdmin](https://www.pgadmin.org/) for database management
- [Postman](https://www.postman.com/) or [Insomnia](https://insomnia.rest/) for API testing
- [kubectl](https://kubernetes.io/docs/tasks/tools/) for Kubernetes deployments

## Quick Start (5 minutes)

### 1. Clone the Repository

```bash
git clone https://github.com/vitconhamchoi/Data-governance.git
cd Data-governance/implementation/dotnet
```

### 2. Start Infrastructure Services

Start PostgreSQL, Redis, and other required services using Docker Compose:

```bash
cd deployment/docker
docker-compose up -d
```

Wait for all services to be healthy:

```bash
docker-compose ps
```

### 3. Restore Dependencies

```bash
cd ../..
dotnet restore
```

### 4. Build the Solution

```bash
dotnet build
```

### 5. Run Database Migrations

```bash
cd src/Services/DataGovernance.API
dotnet ef database update
```

### 6. Run the API

```bash
dotnet run
```

The API should now be running at:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001
- Swagger UI: https://localhost:5001/swagger

## Detailed Setup

### Database Configuration

#### Using Docker (Recommended for Development)

The `docker-compose.yml` file includes PostgreSQL with the following default credentials:

```
Host: localhost
Port: 5432
Database: datagovernance
Username: postgres
Password: postgres
```

#### Using an Existing PostgreSQL Instance

1. Update the connection string in `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=your-host;Database=datagovernance;Username=your-user;Password=your-password"
  }
}
```

2. Create the database:

```sql
CREATE DATABASE datagovernance;
```

3. Run migrations:

```bash
dotnet ef database update --project src/Services/DataGovernance.API
```

### Running Tests

#### Run all tests:

```bash
dotnet test
```

#### Run with coverage:

```bash
dotnet test /p:CollectCoverage=true /p:CoverageReportFormat=opencover
```

#### Run specific test category:

```bash
# Unit tests only
dotnet test --filter "Category=Unit"

# Integration tests only
dotnet test --filter "Category=Integration"
```

### API Documentation

Once the application is running, you can access:

#### Swagger UI
Navigate to: https://localhost:5001/swagger

This provides an interactive API documentation where you can:
- View all available endpoints
- Test API calls directly from the browser
- See request/response schemas

#### OpenAPI Specification
Download the OpenAPI spec from: https://localhost:5001/swagger/v1/swagger.json

### Sample API Requests

#### Create a Data Asset

```bash
curl -X POST https://localhost:5001/api/v1/dataassets \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Customer Table",
    "qualifiedName": "postgres.public.customers",
    "description": "Main customer data table",
    "assetType": "Table",
    "platform": "PostgreSQL",
    "uri": "postgresql://localhost:5432/datagovernance",
    "classification": "Internal",
    "zone": "Trusted",
    "owner": "data-team@example.com",
    "tenantId": "00000000-0000-0000-0000-000000000001",
    "tags": ["customer", "pii", "core"]
  }'
```

#### Get All Data Assets

```bash
curl https://localhost:5001/api/v1/dataassets
```

#### Search Data Assets

```bash
curl "https://localhost:5001/api/v1/dataassets/search?query=customer"
```

## Development Workflow

### 1. Making Code Changes

The solution follows Clean Architecture principles:

```
src/
├── Core/              # Business logic (no dependencies on infrastructure)
│   ├── Domain/        # Entities, interfaces
│   ├── Application/   # Use cases, business logic
│   └── Contracts/     # DTOs, shared contracts
├── Infrastructure/    # External concerns
│   ├── Data/         # EF Core, repositories
│   ├── Messaging/    # Kafka, RabbitMQ
│   └── Cache/        # Redis
└── Services/         # API endpoints
    └── API/          # REST/GraphQL APIs
```

### 2. Adding a New Entity

1. Create entity in `Domain/Entities/`
2. Add DbSet in `DataGovernanceDbContext`
3. Configure in `OnModelCreating`
4. Create migration:
   ```bash
   dotnet ef migrations add AddNewEntity --project src/Infrastructure/DataGovernance.Infrastructure.Data
   ```
5. Apply migration:
   ```bash
   dotnet ef database update --project src/Services/DataGovernance.API
   ```

### 3. Adding a New API Endpoint

1. Create controller in `API/Controllers/`
2. Inject required dependencies
3. Add XML documentation comments
4. Test using Swagger UI

### 4. Hot Reload During Development

The .NET SDK supports hot reload for rapid development:

```bash
dotnet watch run --project src/Services/DataGovernance.API
```

Changes to code will automatically rebuild and restart the application.

## Troubleshooting

### Port Already in Use

If ports 5000/5001 are already in use, you can change them in `appsettings.json`:

```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5002"
      },
      "Https": {
        "Url": "https://localhost:5003"
      }
    }
  }
}
```

### Database Connection Issues

1. Ensure PostgreSQL is running:
   ```bash
   docker-compose ps postgres
   ```

2. Check PostgreSQL logs:
   ```bash
   docker-compose logs postgres
   ```

3. Test connection:
   ```bash
   psql -h localhost -U postgres -d datagovernance
   ```

### Docker Services Not Starting

1. Check Docker is running:
   ```bash
   docker ps
   ```

2. Check for port conflicts:
   ```bash
   docker-compose down
   docker-compose up -d
   ```

3. View logs for specific service:
   ```bash
   docker-compose logs <service-name>
   ```

## Production Deployment

### Building Docker Image

```bash
docker build -t datagovernance-api:latest -f deployment/docker/Dockerfile.api .
```

### Deploying to Kubernetes

```bash
kubectl apply -f deployment/kubernetes/
```

### Environment Variables

For production, configure the following environment variables:

```bash
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=<production-db-connection>
ConnectionStrings__Redis=<production-redis-connection>
Authentication__Jwt__Secret=<secure-secret-key>
```

## Next Steps

- Read the [Architecture Documentation](../../README.md)
- Explore the [API Documentation](docs/api/)
- Review [Architecture Decision Records](docs/architecture/)
- Check out [Sample Applications](samples/)

## Getting Help

- GitHub Issues: [Create an issue](https://github.com/vitconhamchoi/Data-governance/issues)
- Documentation: [Main README](README.md)

## License

See the main repository [LICENSE](../../../LICENSE) file.
