namespace EmployeeApi.Models;

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public int? DepartmentId { get; set; }
}

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

// Request body for Create / Update (so clients cannot set Id)
public record EmployeeDto(string Name, string Email, string Position, decimal Salary, int? DepartmentId);
