# Tenders API

.NET Web API built as a **recruitment project**, focused on clean code, good architecture and production-ready practices.

The application aggregates public tenders data, stores it locally and exposes query endpoints with filtering, sorting and pagination.

---

## Architecture

The solution follows **Clean Architecture** principles and is divided into clear layers:

```
Tenders
│
├── src
│   ├── Tenders.Domain
│   ├── Tenders.Application
│   ├── Tenders.Infrastructure
│   └── Tenders.Api
│
└── tests
    ├── Tenders.DomainTests
    ├── Tenders.ApplicationTests
    └── Tenders.InfrastructureTests
```

Dependencies flow strictly inward:

```
API → Application → Domain
        ↑
   Infrastructure
```

---

## Requirements

To run the project locally you need:

- **.NET 9 SDK**
- Any IDE or editor supporting .NET (Visual Studio / Rider / VS Code)

No additional services or databases are required.

---

## Key Features

- Clean Architecture & SOLID
- ASP.NET Core Web API
- Clear separation of concerns
- Query-based API (filtering, sorting, pagination)
- String-based enums in API
- FluentValidation
- In-memory cache with disk fallback
- Unit tests (NUnit, FluentAssertions, Moq)
- Swagger / OpenAPI

---

## Enums & Validation

- Enums are exposed as **strings** in the API (e.g. `asc`, `desc`)
- Numeric enum values are explicitly rejected
- Parsing is handled defensively to avoid common `Enum.TryParse` pitfalls

---

## Testing

The project is covered with unit tests across all layers.

Run tests:

```bash
dotnet test
```

---

## Running the Application

```bash
dotnet run --project src/Tenders.Api
```

Swagger is available in development mode after startup.

---

## Design Notes

- API uses strings instead of numeric enums for better DX
- No domain dependency on infrastructure
- All external concerns are abstracted and mockable
- Focus on readability, predictability and testability

---

## Author

Piotr Szydło

This project was created as a recruitment task with emphasis on clean code and best practices.

