# Gift of the Givers Web Application:

A .NET 8 ASP.NET Core MVC humanitarian-relief platform, extended with Azure Functions for prototype tax-certificate generation and project-update logging.

## Overview

This solution contains two applications:

- `GiftOfTheGivers.Web` - the main ASP.NET Core MVC website.
- `GiftOfTheGivers.Functions` - Azure Functions v4 isolated-worker HTTP endpoints.

The web application provides a public humanitarian platform where visitors can explore relief projects, donate, register as volunteers, and view project updates. Authenticated users receive role-specific donor and employee functionality.

The project uses an EF Core **InMemory** database for prototype/demo persistence, so web data resets when the application restarts.

## Main Features

### Public website

- Home, About and Contact pages.
- Humanitarian project listing and project detail pages.
- Project targets, raised amounts, locations, beneficiaries and updates.
- Donation workflow supporting one-time and recurring donations.
- Donation causes, currency selection and anonymous donations.
- Donation confirmation and certificate pages.
- Volunteer registration with skills and availability.
- Volunteer confirmation/reference numbers.

### Authentication and roles

ASP.NET Core Identity provides authentication and authorization.

- **Donor**: donor dashboard and donation history.
- **Employee**: employee dashboard, volunteer management and project-update management.
- Access-denied handling for protected areas.
- Seeded development accounts and realistic prototype data.

### Donation management

`DonationService` stores donations and generates unique references such as:

```text
GOTG-2026-000001
```

After a donation is saved, the web application can call the Azure Functions API to generate a prototype tax certificate number and validation code. The Function call is best-effort, so the donation can still complete if the Functions host is unavailable.

### Volunteer management

Volunteer submissions are stored in the prototype database and receive references such as:

```text
VOL-2026-00001
```

Employees can view volunteer submissions from the employee area.

### Project updates

Employees can post updates against humanitarian projects. Updates include a title, description, location, date/time, poster name and optional image URL. Updates appear on project detail pages and can also be logged to Azure Table Storage through the Functions layer.

## Azure Functions

The `GiftOfTheGivers.Functions` project contains two HTTP-triggered functions:

| Function | Route | Purpose |
|---|---|---|
| `GenerateTaxCertificate` | `POST /api/certificates/generate` | Generates a prototype certificate number and validation code for a donation. |
| `LogProjectUpdate` | `POST /api/projectupdates/log` | Stores project-update records in Azure Table Storage. |

The web application communicates with both endpoints through the typed client in:

```text
GiftOfTheGivers.Web/Services/FunctionsApiClient.cs
```

The local Functions base URL is configured through `FunctionsApi:BaseUrl` and defaults to:

```text
http://localhost:7071/api/
```

## Storage

### Web application

The web project uses:

- Entity Framework Core 8
- `Microsoft.EntityFrameworkCore.InMemory`
- ASP.NET Core Identity

Application entities include users/roles, donations, volunteers, projects and project updates.

### Azure Functions

`LogProjectUpdate` uses `Azure.Data.Tables` to write records to an Azure Table Storage table named `ProjectUpdateLogs` by default. **Azurite** can be used as the local storage emulator.

## Project Structure

```text
GiftOfTheGivers.sln
│
├── GiftOfTheGivers.Web/
│   ├── Controllers/
│   ├── Data/
│   ├── Models/
│   ├── Services/
│   ├── Views/
│   ├── wwwroot/
│   ├── Program.cs
│   ├── appsettings.json
│   └── GiftOfTheGivers.Web.csproj
│
├── GiftOfTheGivers.Functions/
│   ├── Functions/
│   ├── Models/
│   ├── Program.cs
│   ├── host.json
│   └── GiftOfTheGivers.Functions.csproj
│
└── README.md
```

## Technology Stack

| Technology | Usage |
|---|---|
| C# | Application/backend development |
| .NET 8 | Runtime and target framework |
| ASP.NET Core MVC | Main web framework |
| Razor Views | Server-rendered UI |
| Entity Framework Core 8 | Data access |
| EF Core InMemory | Prototype persistence |
| ASP.NET Core Identity | Authentication and roles |
| Azure Functions v4 | Serverless HTTP endpoints |
| .NET Isolated Worker | Functions execution model |
| Azure Table Storage | Project-update logging |
| Azurite | Local Azure Storage emulator |
| Bootstrap | Responsive UI |
| jQuery / jQuery Validation | Client-side validation |

## Local Setup

### Prerequisites

Install:

1. .NET 8 SDK
2. Visual Studio 2022 or another .NET 8 compatible IDE
3. Azure Functions Core Tools
4. Azurite if you want to test Table Storage locally

### Run the project

Clone the repository:

```bash
git clone https://github.com/Nethomboni/GiftOfTheGivers.git
cd GiftOfTheGivers
dotnet restore
```

For local Azure Functions configuration, copy the example settings file:

```text
GiftOfTheGivers.Functions/local.settings.example.json
```

to:

```text
GiftOfTheGivers.Functions/local.settings.json
```

Start Azurite when testing the Table Storage integration:

```bash
azurite
```

In Visual Studio, configure both projects as startup projects:

- `GiftOfTheGivers.Web`
- `GiftOfTheGivers.Functions`

The Functions host normally runs on `http://localhost:7071` and the web application calls its `/api/` endpoints.

## Demo Flows

### Donation + certificate

1. Open the donation page.
2. Complete and submit a donation.
3. The donation is saved by the web application.
4. `GenerateTaxCertificate` is called.
5. The returned certificate number and validation code are stored with the donation.
6. Open the Certificate page to view the generated information.

### Employee project update

1. Sign in with an Employee account.
2. Open the Employee Dashboard.
3. Navigate to Project Updates.
4. Create an update for a project.
5. The update is saved to the web application's in-memory database.
6. `LogProjectUpdate` is called.
7. With Azurite running, the record can be inspected through Azure Storage Explorer.

## Development Seed Accounts

The application seeds demo accounts from configuration, with fallback development values in `SeedData.cs`.

| Role | Email | Password |
|---|---|---|
| Employee | `employee@giftofthegivers.local` | `Employee@123` |
| Donor | `donor@giftofthegivers.local` | `Donor@123` |

**These credentials are for development/demo use only. Override them before any production deployment.**

## Configuration

The web application uses:

```json
{
  "FunctionsApi": {
    "BaseUrl": "http://localhost:7071/api/"
  }
}
```

For Azure deployment, change this to the deployed Function App URL, for example:

```text
https://<your-function-app>.azurewebsites.net/api/
```

The Function App also requires Azure Storage configuration for Table Storage.

## Deployment Notes

### ASP.NET Core Web App

The web application can be deployed to an appropriate .NET 8 hosting environment such as Azure App Service.

Before production use:

- Replace EF Core InMemory with persistent relational storage.
- Remove/replace seeded development credentials.
- Store secrets in secure configuration.
- Configure the production Functions API URL.
- Review authentication, authorization, HTTPS and logging settings.

### Azure Functions

Deploy `GiftOfTheGivers.Functions` as a .NET 8 isolated-worker Azure Function App. Configure it with a real Azure Storage connection string and the required table name.

## Prototype Limitations

This is a development/prototype implementation:

- Web data is lost when the application restarts.
- The tax certificate endpoint generates prototype certificate information, not a legally issued certificate.
- Function calls are best-effort so the main demo remains usable if the Functions host is unavailable.
- Production payment processing, persistent storage, secure secret management, monitoring and additional hardening would be required before real-world deployment.

## Additional Documentation

`PART2_SECTION_A_README.md` contains the original Part 2 Section A documentation, including the Azure Functions integration, local setup and deployment notes.

## Author

**Mpho Nethomboni**

GitHub: https://github.com/Nethomboni
