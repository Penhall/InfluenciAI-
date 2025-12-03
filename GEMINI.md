# Project Overview

This is a .NET project called "InfluenciAI", a social media management platform. It's built using Clean Architecture, CQRS, and Event-Driven patterns.

## Main Technologies

*   **.NET 9.0**
*   **C#**
*   **ASP.NET Core Minimal API**
*   **Entity Framework Core**
*   **PostgreSQL**
*   **Redis**
*   **RabbitMQ**
*   **WPF (Windows Presentation Foundation)**
*   **MVVM (Model-View-ViewModel)**

## Architecture

The project follows the principles of **Clean Architecture** and is structured as follows:

*   `src/Core/Domain`: Contains the domain entities and value objects.
*   `src/Core/Application`: Implements the application logic using CQRS (Command Query Responsibility Segregation) with MediatR.
*   `src/Infra/Infrastructure`: Handles data persistence with Entity Framework Core, identity management, and integrations with external services.
*   `src/Server/Api`: Exposes a RESTful API built with ASP.NET Core Minimal API.
*   `src/Client/Desktop`: A desktop application built with WPF and the MVVM pattern.

# Building and Running

Here are the key commands for building, running, and testing the project:

## Dependencies

The project relies on the following services, which are managed via Docker Compose:

*   **PostgreSQL:** Database
*   **Redis:** Caching
*   **RabbitMQ:** Messaging

To start these services, run:

```bash
docker compose up -d
```

## Running the Application

1.  **Set up secrets:**
    ```powershell
    .\scripts\setup-secrets.ps1
    ```
2.  **Apply database migrations:**
    ```bash
    dotnet ef database update --project src/Infra/InfluenciAI.Infrastructure --startup-project src/Server/InfluenciAI.Api
    ```
3.  **Run the API:**
    ```bash
    dotnet run --project src/Server/InfluenciAI.Api
    ```
4.  **Run the Desktop application:**
    ```bash
    dotnet run --project src/Client/InfluenciAI.Desktop
    ```

# Development Conventions

*   **Coding Style:** The project follows the standard C# and .NET coding conventions.
*   **Testing:** The `tests` directory contains unit and integration tests for the project.
*   **Contribution:** See the `.rules/` directory for more information on development standards and style guides.
