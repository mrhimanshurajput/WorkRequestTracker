# Work Request Tracker
A practical assignment implementation using ASP.NET Core Web API (.NET 8), React + Vite, and SQL Server.

## Features
- Paginated list of work requests with status filter and title/client search
- Get request by ID
- Create work request
- Update request status
- Add notes to a request
- Validation and consistent API error responses
- SQL Server schema with indexes
- React loading and error states

## Prerequisites
- .NET 8 SDK
- Node.js 18+ and npm
- SQL Server (LocalDB or SQL Server Developer Edition) if using persistent storage

## Run the backend
1. Update `backend/WorkRequestTracker.Api/appsettings.json` with your SQL Server connection string.
2. Open a terminal in `backend/WorkRequestTracker.Api`.
3. Run:
   ```bash
   dotnet restore
   dotnet run
   ```
4. The API uses `http://localhost:5080` and `https://localhost:7080` by default (actual ports may be printed by `dotnet run`).

The application calls `Database.EnsureCreated()` at startup for a convenient demo. For a real deployment, use EF Core migrations instead. If the database cannot be reached, configure SQL Server before running.

## Run the frontend
1. Open another terminal in `frontend`.
2. Run:
   ```bash
   npm install
   npm run dev
   ```
3. Open the URL printed by Vite, usually `http://localhost:5173`.
4. The frontend expects the API at `http://localhost:5080`. Set `VITE_API_BASE_URL` in `frontend/.env` if needed.

## API endpoints
- `GET /api/work-requests?status=&search=&page=1&pageSize=20`
- `GET /api/work-requests/{id}`
- `POST /api/work-requests`
- `PATCH /api/work-requests/{id}/status`
- `POST /api/work-requests/{id}/notes`

## Example create request
```json
{
  "title": "Review vendor invoice",
  "clientName": "Northwind Ltd",
  "description": "Validate the invoice against the purchase order.",
  "priority": "High",
  "status": "New",
  "dueDate": "2026-11-01",
  "notes": "Initial request"
}
```

## Notes / limitations
- This is a focused assignment sample, not a production-ready service.
- Authentication/authorization is not implemented; add the platform's identity provider and per-request access checks before production use.
- No automated test project is included in this time-boxed sample.
- EnsureCreated is for local demo convenience; use migrations and a deployment pipeline for production.
- No status-transition workflow rules are enforced beyond validating allowed status values.
- Configure a real SQL Server connection string before running with persistent storage.

## Candidate
Himanshu Premsing Rajput
