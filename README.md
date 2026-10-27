# employee-management-api-async

ASP.NET Core Web API (.NET 8) with EF Core (SQLite) demonstrating:

- **Async/Await** CRUD for an `Employee` API
- **CancellationToken** passed from the controller through the service down to EF Core
- **Performance measurement**: sequential vs parallel execution with `Stopwatch`

## Tech stack

- .NET 8, ASP.NET Core Web API
- Entity Framework Core 8 + SQLite (no database server needed)
- Swagger UI

## Getting started

```bash
dotnet restore
dotnet run
```

Open `/swagger` on the URL printed in the console. The database file (`employees.db`) is created and seeded automatically on first run.

## Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/employees` | Get all employees |
| GET | `/api/employees/{id}` | Get one employee |
| POST | `/api/employees` | Create an employee |
| PUT | `/api/employees/{id}` | Update an employee |
| DELETE | `/api/employees/{id}` | Delete an employee |
| GET | `/api/performance/compare?delayMs=1000` | Compare sequential vs parallel execution |

Example request body for POST / PUT:

```json
{
  "name": "Mona Samir",
  "email": "mona@example.com",
  "position": "QA Engineer",
  "salary": 10000,
  "departmentId": 1
}
```

## Project structure

```
Controllers/   EmployeesController, PerformanceController
Services/      IEmployeeService, EmployeeService
Models/        Employee, Department, EmployeeDto
Data/          AppDbContext
Program.cs     DI setup, database creation and seeding
```

## Notes on the design

- **Task / Task<T>**: controller actions return `Task<ActionResult<T>>`; service methods return `Task` / `Task<T>`. `GetByIdAsync` returns the EF Core task directly (no `async/await` needed because there is no work after the I/O call).
- **CancellationToken**: every endpoint and service method accepts one and forwards it to `ToListAsync`, `FirstOrDefaultAsync` and `SaveChangesAsync`. Cancellation does not guarantee the database stops immediately; that depends on the provider.
- **Parallel test**: each parallel operation creates its own `DbContext` via `IDbContextFactory<AppDbContext>`, because EF Core does not support running multiple operations in parallel on the same `DbContext` instance.
- **Simulated latency**: `delayMs` adds a `Task.Delay` to each operation to simulate a remote database. With a local SQLite file the queries finish in a fraction of a millisecond, so parallelism would show almost no difference without it.

## Performance report

Run `GET /api/performance/compare?delayMs=1000` and record the results:

| Method | Time (ms) |
|--------|-----------|
| Sequential | |
| Parallel (`Task.WhenAll`) | |

With two independent operations of about 1 second each, sequential is expected to take about 2 seconds and parallel about 1 second. Parallelism helps for independent I/O-bound work; it does not help CPU-bound work or operations that depend on each other.
