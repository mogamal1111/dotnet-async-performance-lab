using EmployeeApi.Models;
using EmployeeApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController(IEmployeeService service) : ControllerBase
{
    // GET api/employees
    [HttpGet]
    public async Task<ActionResult<List<Employee>>> GetAll(CancellationToken cancellationToken)
    {
        var employees = await service.GetAllAsync(cancellationToken);
        return Ok(employees);
    }

    // GET api/employees/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Employee>> Get(int id, CancellationToken cancellationToken)
    {
        var employee = await service.GetByIdAsync(id, cancellationToken);
        return employee is null ? NotFound() : Ok(employee);
    }

    // POST api/employees
    [HttpPost]
    public async Task<ActionResult<Employee>> Create(EmployeeDto dto, CancellationToken cancellationToken)
    {
        var created = await service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    // PUT api/employees/5
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Employee>> Update(int id, EmployeeDto dto, CancellationToken cancellationToken)
    {
        var updated = await service.UpdateAsync(id, dto, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    // DELETE api/employees/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await service.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
