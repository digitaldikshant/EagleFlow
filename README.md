# EagleFlow - PDF Document Retrieval System

This MVC application implements phase-1 scope for PDF upload, retrieval, download, and soft delete.

## Stack
- ASP.NET Core MVC
- Entity Framework Core
- SQL Server (optional connection string) or in-memory fallback for local demo

## Run
```bash
dotnet restore
dotnet run
```

## Key Routes
- Public retrieval page: `/Public/Index`
- Admin page: `/Admin/Index`
- View document: `/Public/ViewDocument/{documentNumber}`
- Download document: `/Public/Download/{documentNumber}`

## Notes
- Upload restricted to PDF and max 10 MB.
- Documents are stored in `App_Data/Documents`.
- Document numbers are random alphanumeric with `DOC-` prefix.
- Soft-deleted documents are hidden from public endpoints.
