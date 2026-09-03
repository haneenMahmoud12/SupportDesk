# SupportDesk

SupportDesk is an ASP.NET Core REST API for managing customer-support tickets. It includes JWT authentication, role-based authorization, ticket comments, filtering, sorting, search, pagination, consistent API responses, and centralized exception handling.

## Features

- Registration and login with ASP.NET Core Identity
- JWT bearer authentication
- `User` and `Admin` roles
- Users manage their own tickets and comments
- Administrators view and manage all tickets and change ticket status
- Ticket statuses: `Open`, `InProgress`, and `Closed`
- Ticket priorities: `Low`, `Medium`, and `High`
- Search by ticket title or description
- Filtering by status and priority
- Sorting and pagination
- DTO validation and consistent error responses
- Repository-based data access with EF Core and SQL Server
- Centralized exception handling and structured `ILogger` logging
- Swagger/OpenAPI documentation with JWT authorization

## Architecture

```text
SupportDesk.Api             Controllers, middleware, configuration and Swagger
SupportDesk.Application     DTOs, service contracts, services and response models
SupportDesk.Domain          Entities and enums
SupportDesk.Infrastructure  Identity, EF Core, repositories, migrations and auth
Tests                       Unit and integration test project
```

## Prerequisites

- .NET 10 SDK
- SQL Server
- A valid SQL Server connection string

## Configuration

The API requires the following settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_SQL_SERVER_CONNECTION_STRING"
  },
  "Jwt": {
    "Issuer": "SupportDesk.Api",
    "Audience": "SupportDesk.Client",
    "AccessTokenExpirationMinutes": 60,
    "Key": "A_SECRET_KEY_OF_AT_LEAST_32_BYTES"
  },
  "AdminUser": {
    "Email": "admin@example.com",
    "Password": "A_STRONG_ADMIN_PASSWORD"
  }
}
```

Do not commit real passwords or signing keys. For local development, store them with .NET User Secrets:

```powershell
dotnet user-secrets init --project SupportDesk.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_CONNECTION_STRING" --project SupportDesk.Api
dotnet user-secrets set "Jwt:Key" "YOUR_SECRET_KEY_OF_AT_LEAST_32_BYTES" --project SupportDesk.Api
dotnet user-secrets set "AdminUser:Email" "admin@example.com" --project SupportDesk.Api
dotnet user-secrets set "AdminUser:Password" "YOUR_STRONG_PASSWORD" --project SupportDesk.Api
```

At startup, the application creates the `Admin` and `User` roles. If the optional `AdminUser` settings are provided, it also creates the configured administrator and assigns the `Admin` role.

## Database setup

Restore dependencies and apply the existing migrations:

```powershell
dotnet restore SupportDesk.slnx
dotnet ef database update --project SupportDesk.Infrastructure --startup-project SupportDesk.Api
```

If the EF Core CLI is unavailable:

```powershell
dotnet tool install --global dotnet-ef
```

## Run the API

```powershell
dotnet run --project SupportDesk.Api
```

The development profiles use:

- HTTPS: `https://localhost:7280`
- HTTP: `http://localhost:5107`
- Swagger: `https://localhost:7280/swagger`

## Use JWT authentication in Swagger

1. Call `POST /api/auth/register` to create a regular user, if needed.
2. Call `POST /api/auth/login` with the email and password.
3. Copy the returned `accessToken`.
4. Select **Authorize** at the top of Swagger.
5. Paste the token without adding `Bearer`; Swagger adds that prefix.
6. Select **Authorize** and call a protected endpoint.

## API endpoints

| Method | Endpoint | Access | Purpose |
|---|---|---|---|
| `POST` | `/api/auth/register` | Anonymous | Register a user |
| `POST` | `/api/auth/login` | Anonymous | Obtain a JWT |
| `GET` | `/api/tickets/ticket/{id}` | Authenticated | Get an accessible ticket |
| `GET` | `/api/tickets/tickets` | Admin | Get all tickets |
| `GET` | `/api/tickets/userTickets` | Authenticated | Get the current user's tickets |
| `POST` | `/api/tickets/saveTicket` | Authenticated | Create or update a ticket |
| `DELETE` | `/api/tickets/delete/{id}` | Authenticated | Soft-delete a ticket |
| `PATCH` | `/api/tickets/{id}/status` | Admin | Change ticket status |
| `GET` | `/api/tickets/{ticketId}/comments` | Authenticated | Get comments |
| `POST` | `/api/tickets/{ticketId}/comments` | Authenticated | Create or update a comment |
| `DELETE` | `/api/tickets/{ticketId}/comments/{id}` | Authenticated | Soft-delete a comment |

List endpoints accept `pageNumber`, `pageSize`, `search`, `sortColumn`, and `sortDirection`. Ticket lists also accept `status` and `priority`.

Example:

```http
GET /api/tickets/userTickets?pageNumber=1&pageSize=10&status=Open&priority=High&sortColumn=createdAt&sortDirection=desc&search=login
Authorization: Bearer YOUR_ACCESS_TOKEN
```

## Response format

All handled errors use the common response model. For example:

```json
{
  "succeeded": false,
  "errors": [
    "Ticket not found."
  ]
}
```

Unhandled exceptions return HTTP 500 with a generic message. Exception details are logged with the request trace identifier and are not exposed to clients.

## Logging

The application uses ASP.NET Core `ILogger`. Logs appear in the terminal and Visual Studio Output window by default. Log levels are configured under `Logging:LogLevel`. A persistent file provider is not currently configured.

## Tests

Run the test project with:

```powershell
dotnet test SupportDesk.slnx
```

The test project is scaffolded, but test cases still need to be implemented.
