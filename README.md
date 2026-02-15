# EagleFlow - PDF Document Retrieval System

This MVC application implements phase-1 scope for PDF upload, retrieval, download, and soft delete.

## Stack
- ASP.NET Core MVC (.NET 8)
- Entity Framework Core
- SQL Server (optional connection string) or in-memory fallback for local demo

## Admin Authentication
- Admin panel is protected with cookie authentication.
- Admin login uses email + password.
- Seed admin is created on first run from `appsettings*.json` (`AdminAuth:SeedAdmin`).
- Forgot password flow supports OTP verification using:
  - Email OTP
  - Mobile OTP (simulated SMS logger by default; integrate provider later)

## Upload Notification
- After successful PDF upload, a success email with generated document number is sent to logged-in admin email.
- If SMTP is not configured, emails are logged (simulation mode).

## Run
```bash
dotnet restore
dotnet run
```

## Key Routes
- Public retrieval page: `/Public/Index`
- Admin page: `/Admin/Index` (requires login)
- Admin login: `/Account/Login`
- Forgot password: `/Account/ForgotPassword`
- View document: `/Public/ViewDocument/{documentNumber}`
- Download document: `/Public/Download/{documentNumber}`

## Notes
- Upload is restricted to PDF extension, 10 MB max, and PDF file signature (`%PDF-`) validation.
- Documents are stored in `App_Data/Documents`.
- Document numbers are random alphanumeric with `DOC-` prefix.
- Soft-deleted documents are hidden from public endpoints.
