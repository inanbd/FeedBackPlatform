# Feedback Platform

A simple, self-hosted feedback collection platform for your websites and apps. Register an
application, generate an API key, and start collecting star ratings, comments, and custom fields
with a single `curl` command. Built with ASP.NET Core 10 using Clean Architecture — no Entity
Framework, no ASP.NET Identity.

## Features

- **Admin portal** — admins see every user's applications and feedback, and configure API rate limits.
- **Self-service accounts** — anyone can register and manage their own applications and feedback.
- **Simple ingestion API** — `POST /api/v1/feedback` accepts app version, title, comment, star
  rating, plus optional reporter name/contact. The caller's IP address is recorded automatically.
- **Custom feedback fields** — each application can define its own extra fields (text, number,
  boolean, date, or a fixed option list); required fields are enforced at submission time.
- **Dashboard** — feedback volume over time and star-rating distribution, per application or across all of them.
- **Configurable rate limiting** — a platform-wide default (admin-only), with optional per-application overrides.
- **Email notifications** — the application owner gets an email for every new feedback submission (SMTP settings in `appsettings.json`).
- **SQL Server or SQLite** — pick the provider in configuration; the same code path runs either database.
- **Developer & user documentation** — see the `/Docs` page once the app is running.

## Architecture

Clean Architecture, four layers:

```
src/
  FeedbackPlatform.Domain          Entities & enums only, no dependencies
  FeedbackPlatform.Application     Use cases (MediatR commands/queries), interfaces, validation
  FeedbackPlatform.Infrastructure  Dapper repositories, SQL Server/SQLite, email (MailKit), security
  FeedbackPlatform.Web             ASP.NET Core MVC portal + the feedback ingestion API
tests/
  FeedbackPlatform.Application.Tests
```

- **Data access**: [Dapper](https://github.com/DapperLib/Dapper) over raw ADO.NET — no EF Core. The
  schema is created by hand-written, idempotent SQL scripts (`Infrastructure/Persistence/Scripts/`)
  run once at startup; there's no migration history since there's nothing to migrate between yet.
- **CQRS**: [MediatR](https://github.com/jbogard/MediatR) commands/queries in the Application layer, with a
  FluentValidation pipeline behavior.
- **Auth**: cookie authentication for the portal (custom PBKDF2 password hashing — no ASP.NET
  Identity), and a separate API-key scheme (`X-Api-Key` header) for the ingestion API.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQLite (default, zero setup) or a SQL Server instance if you prefer that provider

## Running it

```bash
cd src/FeedbackPlatform.Web
dotnet run
```

The app creates its SQLite database file and schema automatically on first run, and seeds an
admin account from the `Seed` section of `appsettings.json` (defaults to
`admin@example.com` / `ChangeMe123!` — **change this before deploying anywhere real**).

Once running:
- Portal: `http://localhost:5000` (register a regular account, or log in as the seeded admin)
- Docs: `http://localhost:5000/Docs`
- Swagger (Development only): `http://localhost:5000/swagger`

## Configuration (`appsettings.json`)

### Database

```json
"Database": {
  "Provider": "Sqlite",
  "ConnectionString": "Data Source=feedbackplatform.db"
}
```

To use SQL Server instead:

```json
"Database": {
  "Provider": "SqlServer",
  "ConnectionString": "Server=localhost;Database=FeedbackPlatform;User Id=sa;Password=Your_password123;TrustServerCertificate=True"
}
```

### Email notifications

```json
"Email": {
  "Enabled": false,
  "SmtpHost": "smtp.example.com",
  "SmtpPort": 587,
  "UseSsl": true,
  "Username": "",
  "Password": "",
  "FromAddress": "noreply@example.com",
  "FromName": "Feedback Platform"
}
```

Set `Enabled` to `true` and fill in your SMTP provider's details to have application owners emailed
on every new feedback submission. Emails are sent from a background queue so the API never blocks
on SMTP.

### First admin account

```json
"Seed": {
  "AdminEmail": "admin@example.com",
  "AdminPassword": "ChangeMe123!",
  "AdminDisplayName": "Administrator"
}
```

Created once, on first startup, if no user with that email already exists.

### Rate limiting

The default requests-per-minute limit applied to every API key is stored in the database (not
`appsettings.json`) so admins can change it at runtime from **Admin → Settings** in the portal,
without restarting the app. Individual applications can also get their own override from
**Admin → Applications**.

## Using the API

```bash
curl -X POST http://localhost:5000/api/v1/feedback \
  -H "X-Api-Key: fbk_your-generated-key" \
  -H "Content-Type: application/json" \
  -d '{
        "appVersion": "1.4.2",
        "title": "Great app!",
        "comment": "Loving it so far.",
        "starRating": 5
      }'
```

See the in-app `/Docs` page for the full reference, including custom fields and error responses.

## Tests

```bash
dotnet test
```
