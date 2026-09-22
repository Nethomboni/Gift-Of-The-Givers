# Part 2, Section A — Extending the Web App with Azure Functions

## What was added

A new project, **GiftOfTheGivers.Functions** (isolated worker, .NET 8, Azure
Functions v4), sitting alongside GiftOfTheGivers.Web in the same solution
(`GiftOfTheGivers.sln`).

It has an HTTP Trigger template project (A.1) with two functions (A.2):

| Function | Route | Triggered by | What it does |
|---|---|---|---|
| `GenerateTaxCertificate` | `POST /api/certificates/generate` | A donor completing the donation form | Returns a dummy certificate number + validation code |
| `LogProjectUpdate` | `POST /api/projectupdates/log` | An employee posting a project update | Writes the update to an Azure Table Storage table (`ProjectUpdateLogs`) |

The Web project calls both, through a small typed client
(`Services/IFunctionsApiClient.cs` / `FunctionsApiClient.cs`):

- `DonationController.Index` (POST) calls `GenerateTaxCertificate` right after
  a donation is saved, and stores the returned certificate number/validation
  code on the `Donation` (shown on the Certificate page).
- `EmployeeController.ProjectUpdates` (POST) calls `LogProjectUpdate` right
  after an update is saved to the (in-memory) database.

Both calls are best-effort: if the Functions host isn't running, the web app
still works — the donation still completes, the update still posts — it just
logs a warning and the certificate number/Azure Storage log is skipped. This
keeps Part 1's prototype reliably demoable even if you forget to start the
Functions project.

## Running it locally in Visual Studio

1. Open `GiftOfTheGivers.sln` — it now has two projects.
2. Install the **Azure Storage Emulator (Azurite)** if you don't have it —
   in Visual Studio: `Tools > Options > Azure Storage` or `npm install -g azurite`,
   then run `azurite` in a terminal. `LogProjectUpdate` needs it running
   locally (connection string `UseDevelopmentStorage=true` in
   `GiftOfTheGivers.Functions/local.settings.json`).
3. Right-click the solution → **Set Startup Projects** → **Multiple startup
   projects** → set both `GiftOfTheGivers.Web` and `GiftOfTheGivers.Functions`
   to **Start**.
4. Press F5. The Functions host starts on `http://localhost:7071` and the Web
   app on its usual port; `appsettings.json` already points
   `FunctionsApi:BaseUrl` at `http://localhost:7071/api/`.
5. Demo flow for screenshots:
   - Submit a donation → open its **Certificate** page → the Certificate
     No./Validation Code fields (sourced from the Function) appear.
   - Log in as the seeded Employee account → **Project Updates** → post an
     update → the success banner confirms it was logged to Azure Storage;
     check Azurite (e.g. with **Azure Storage Explorer**) for the
     `ProjectUpdateLogs` table to show the new row.

## Deploying the Function App to Azure

- Publish `GiftOfTheGivers.Functions` to an Azure Function App as usual.
- In that Function App's Configuration, set `StorageConnectionString` to a
  real Storage Account connection string (the Function App's own
  `AzureWebJobsStorage` can be reused, or a separate account).
- In the Web app's configuration (App Service Configuration or
  `appsettings.json`), set `FunctionsApi:BaseUrl` to the deployed Function
  App's URL, e.g. `https://gotg-functions.azurewebsites.net/api/`.
