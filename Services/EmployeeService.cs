using EmployeeApi.Data;
using EmployeeApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeApi.Services;

public class EmployeeService(AppDbContext context) : IEmployeeService
{
    // Task<List<T>> : returns a collection result
    public async Task<List<Employee>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await context.Employees
            .AsNoTracking()
            .OrderBy(e => e.Id)
            .ToListAsync(cancellationToken);
    }

    // Simple pass-through: no async/await needed, the Task is returned directly.
    public Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    // Needs work after the I/O (SaveChanges) -> async/await is required.
    public async Task<Employee> CreateAsync(EmployeeDto dto, CancellationToken cancellationToken)
    {
        var employee = new Employee
        {
            Name = dto.Name,
            Email = dto.Email,
            Position = dto.Position,
            Salary = dto.Salary,
            DepartmentId = dto.DepartmentId
        };

        context.Employees.Add(employee);
        await context.SaveChangesAsync(cancellationToken);
        return employee;
    }

    public async Task<Employee?> UpdateAsync(int id, EmployeeDto dto, CancellationToken cancellationToken)
    {
        var employee = await context.Employees
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (employee is null)
            return null;

        employee.Name = dto.Name;
        employee.Email = dto.Email;
        employee.Position = dto.Position;
        employee.Salary = dto.Salary;
        employee.DepartmentId = dto.DepartmentId;

        await context.SaveChangesAsync(cancellationToken);
        return employee;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var employee = await context.Employees
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (employee is null)
            return false;

        context.Employees.Remove(employee);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
