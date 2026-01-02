# Purchase Service

A REST API service for managing purchase transactions in a microservices architecture. This service handles CRUD operations for purchases and publishes events to RabbitMQ for event-driven communication with other services.

## Overview

The Purchase Service is built with .NET 8.0 and provides a RESTful API for managing purchase records. It integrates with SQL Server for data persistence and RabbitMQ for event-driven messaging. The service also consumes events from other services (Offer and Transport services) to maintain data consistency.

## Features

- **CRUD Operations**: Create, Read, Update, and Delete purchase records
- **Event-Driven Architecture**: Publishes purchase events (Created, Updated) to RabbitMQ
- **Event Consumers**: Consumes events from Offer and Transport services
- **Database Integration**: Uses Entity Framework Core with SQL Server
- **API Documentation**: Swagger/OpenAPI documentation available in development mode
- **Health Checks**: Built-in health check endpoints
- **Observability**: OpenTelemetry integration for distributed tracing
- **Logging**: Structured logging with Serilog
- **Auto-mapping**: AutoMapper for DTO to Entity conversions

## Technology Stack

- **.NET 8.0**: Latest LTS version of .NET
- **FastEndpoints**: High-performance REST API framework
- **Entity Framework Core 8.0**: ORM for database operations
- **SQL Server**: Primary data store
- **MassTransit**: Message bus abstraction for RabbitMQ
- **RabbitMQ**: Message broker for event-driven communication
- **AutoMapper**: Object-to-object mapping
- **Swagger/OpenAPI**: API documentation
- **xUnit**: Unit testing framework
- **Moq**: Mocking framework for tests
- **OpenTelemetry**: Distributed tracing and monitoring
- **Serilog**: Structured logging

## Prerequisites

Before running the application, ensure you have the following installed:

- **.NET 8.0 SDK** or later ([Download](https://dotnet.microsoft.com/download))
- **SQL Server** (LocalDB, Express, or Full version)
- **RabbitMQ** (Optional: Required for event-driven features)
- **Docker** (Optional: For containerized deployment)

## Project Structure

```
purchase-service/
├── src/
│   └── Api/
│       ├── Domain/
│       │   ├── Configuration/     # Service configuration
│       │   ├── Consumers/         # RabbitMQ event consumers
│       │   ├── Data/              # DbContext and database configuration
│       │   ├── DTOs/              # Data Transfer Objects
│       │   ├── Endpoints/         # API endpoint handlers
│       │   ├── Entities/          # Domain entities
│       │   ├── Events/            # Event models for RabbitMQ
│       │   ├── Mappings/          # AutoMapper profiles
│       │   └── Validators/        # Request validators
│       ├── Events/                # External event models
│       ├── Shared/                # Shared utilities
│       └── Program.cs             # Application entry point
├── tests/
│   └── UnitTests/                 # Unit tests
├── Dockerfile                     # Docker configuration
└── AI.PurchaseService.sln        # Solution file
```

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/<your-org>/purchase-service.git
cd purchase-service
```

### 2. Configure the Application

Update the connection string and RabbitMQ settings in `src/Api/appsettings.json` or use environment variables:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_CONNECTION_STRING"
  },
  "Configs": {
    "RabbitMQ": {
      "HostName": "localhost",
      "PortNumber": 5672
    }
  },
  "Secrets": {
    "RabbitMQ": {
      "Username": {username},
      "UserPassword": {password}
    }
  }
}
```

### 3. Running Locally

#### Option A: Using .NET CLI

```bash
# Build the solution
dotnet build

# Run the API
cd src/Api
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `http://localhost:5000/swagger` (Development mode only)

#### Option B: Using Docker

```bash
# Build the Docker image
docker build -t purchase-service:latest .

# Run the container
docker run -d \
  -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="YOUR_CONNECTION_STRING" \
  -e Configs__RabbitMQ__HostName="YOUR_RABBITMQ_HOST" \
  --name purchase-service \
  purchase-service:latest
```

The API will be available at `http://localhost:8080`

**Note:** For production deployments, use Docker secrets or mounted configuration files instead of environment variables to protect sensitive data.

### 5. Running with Docker Compose (Optional)

For a complete setup with SQL Server and RabbitMQ, you can create a `docker-compose.yml` file with the required services. Example:

```yaml
version: '3.8'
services:
  purchase-service:
    build: .
    ports:
      - "8080:8080"
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=PurchaseServiceDB;User Id=sa;Password=<YOUR_STRONG_PASSWORD>;TrustServerCertificate=true
      - Configs__RabbitMQ__HostName=rabbitmq
    depends_on:
      - sqlserver
      - rabbitmq
      
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=<YOUR_STRONG_PASSWORD>
    ports:
      - "1433:1433"
      
  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"
      - "15672:15672"
```

Then run:

```bash
docker-compose up -d
```

**Important:** Replace `<YOUR_STRONG_PASSWORD>` with a secure password. For production environments, use Docker secrets or external secret management tools instead of hardcoded credentials.

## API Endpoints

The service provides the following REST API endpoints:

### Purchase Operations

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/purchase-service/api/v1/purchases` | Create a new purchase |
| GET | `/purchase-service/api/v1/purchases` | Get all purchases |
| GET | `/purchase-service/api/v1/purchases/{id}` | Get a specific purchase by ID |
| PUT | `/purchase-service/api/v1/purchases/{id}` | Update a purchase |
| DELETE | `/purchase-service/api/v1/purchases/{id}` | Delete a purchase |

### Example Request (Create Purchase)

```bash
curl -X POST http://localhost:5000/purchase-service/api/v1/purchases \
  -H "Content-Type: application/json" \
  -d '{
    "buyer_id": 1,
    "offer_id": 100,
    "transport_id": 50,
    "assigned_at": "2024-01-01T10:00:00Z",
    "bid_amount": 1500.00,
    "status": "Assigned"
  }'
```

### Example Response

```json
{
  "id": 1,
  "buyer_id": 1,
  "offer_id": 100,
  "transport_id": 50,
  "assigned_at": "2024-01-01T10:00:00Z",
  "bid_amount": 1500.00,
  "status": "Assigned",
  "created_at": "2024-01-01T10:00:00Z",
  "last_modified_at": "2024-01-01T10:00:00Z"
}
```

## API Documentation

When running in Development mode, Swagger UI is available at:
- `[http://localhost:5000/swagger/index.html](http://13.204.74.15:8082/swagger/index.html#/Purchase-Service)`

This provides interactive API documentation where you can test endpoints directly.

## Running Tests

The project includes unit tests using xUnit.

```bash
# Run all tests
dotnet test

# Run tests with code coverage
dotnet test /p:CollectCoverage=true /p:CoverageReportsFormat=opencover

# Run tests in a specific project
cd tests/UnitTests
dotnet test
```

## Event-Driven Architecture

The service participates in an event-driven architecture:

### Published Events

- **PurchaseCreatedEvent**: Published when a new purchase is created
- **PurchaseUpdatedEvent**: Published when a purchase is updated

### Consumed Events

- **OfferCreatedEvent**: Consumed from the Offer service
- **OfferUpdatedEvent**: Consumed from the Offer service
- **TransportCreatedEvent**: Consumed from the Transport service

All events are exchanged via RabbitMQ using MassTransit.

## Health Checks

The service includes health check endpoints for monitoring:

- `/health`: Basic health check
- `/health/ready`: Readiness check (includes database connectivity)

## Support

For issues and questions, please open an issue in the GitHub repository.
