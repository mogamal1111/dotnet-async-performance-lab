using System.Diagnostics;
using EmployeeApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PerformanceController(IDbContextFactory<AppDbContext> factory) : ControllerBase
{
    // GET api/performance/compare?delayMs=1000
    // delayMs simulates a slow remote database / service (network latency) for each operation.
    [HttpGet("compare")]
    public async Task<IActionResult> Compare([FromQuery] int delayMs = 1000, CancellationToken cancellationToken = default)
    {
        // Warm-up so JIT / first connection cost does not distort the first measurement.
        await RunSequentialAsync(0, cancellationToken);

        // 1) Sequential: the second operation starts after the first one finishes.
        var sw = Stopwatch.StartNew();
        await RunSequentialAsync(delayMs, cancellationToken);
        sw.Stop();
        var sequentialMs = sw.ElapsedMilliseconds;

        // 2) Parallel: both operations are started first, then awaited together.
        sw.Restart();
        await RunParallelAsync(delayMs, cancellationToken);
        sw.Stop();
        var parallelMs = sw.ElapsedMilliseconds;

        return Ok(new
        {
            delayMsPerOperation = delayMs,
            sequentialMs,
            parallelMs,
            speedup = parallelMs == 0 ? (double?)null : Math.Round((double)sequentialMs / parallelMs, 2),
            parallelWasFaster = parallelMs < sequentialMs
        });
    }

    private async Task RunSequentialAsync(int delayMs, CancellationToken ct)
    {
        await CountEmployeesAsync(delayMs, ct);
        await CountDepartmentsAsync(delayMs, ct);
    }

    private async Task RunParallelAsync(int delayMs, CancellationToken ct)
    {
        var employeesTask = CountEmployeesAsync(delayMs, ct);
        var departmentsTask = CountDepartmentsAsync(delayMs, ct);
        await Task.WhenAll(employeesTask, departmentsTask);
    }

    // Each operation creates its OWN DbContext: EF Core does not allow
    // running parallel operations on the same DbContext instance.
    private async Task<int> CountEmployeesAsync(int delayMs, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        if (delayMs > 0) await Task.Delay(delayMs, ct);
        return await db.Employees.AsNoTracking().CountAsync(ct);
    }

    private async Task<int> CountDepartmentsAsync(int delayMs, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        if (delayMs > 0) await Task.Delay(delayMs, ct);
        return await db.Departments.AsNoTracking().CountAsync(ct);
    }
}
