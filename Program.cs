using EmployeeApi.Data;
using EmployeeApi.Models;
using EmployeeApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Factory (used for the parallel test) + a scoped DbContext created from it (used by the service).
builder.Services.AddDbContextFactory<AppDbContext>(o => o.UseSqlite("Data Source=employees.db"));
builder.Services.AddScoped(sp => sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

builder.Services.AddScoped<IEmployeeService, EmployeeService>();

var app = builder.Build();

// Create the database and seed some data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();

    if (!await db.Departments.AnyAsync())
    {
        db.Departments.AddRange(new Department { Name = "IT" }, new Department { Name = "HR" });
        await db.SaveChangesAsync();
    }
    if (!await db.Employees.AnyAsync())
    {
        db.Employees.AddRange(
            new Employee { Name = "Ahmed Ali", Email = "ahmed@example.com", Position = "Developer", Salary = 12000, DepartmentId = 1 },
            new Employee { Name = "Sara Hassan", Email = "sara@example.com", Position = "HR Specialist", Salary = 9000, DepartmentId = 2 });
        await db.SaveChangesAsync();
    }
}

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
