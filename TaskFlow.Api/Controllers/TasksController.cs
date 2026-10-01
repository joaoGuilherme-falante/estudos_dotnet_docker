using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Contracts;
using TaskFlow.Api.Models;
using TaskFlow.Api.Services;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public sealed class TasksController(ITaskService taskService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TaskResponse>>> GetAll(
        [FromQuery] int? projectId,
        CancellationToken ct) =>
        Ok(await taskService.GetAllAsync(projectId, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskResponse>> GetById(int id, CancellationToken ct) =>
        Ok(await taskService.GetByIdAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<TaskResponse>> Create(CreateTaskRequest request, CancellationToken ct)
    {
        var task = await taskService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TaskResponse>> Update(int id, UpdateTaskRequest request, CancellationToken ct) =>
        Ok(await taskService.UpdateAsync(id, request, ct));

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<TaskResponse>> ChangeStatus(
        int id,
        ChangeTaskStatusRequest request,
        CancellationToken ct)
    {
        var roleClaim = User.FindFirst("role")?.Value;
        if (!Enum.TryParse<UserRole>(roleClaim, out var role))
        {
            throw new DomainException("The authenticated token has no valid role.", 403);
        }

        return Ok(await taskService.ChangeStatusAsync(
            id, request.Status, CurrentUser.Id(User), role, ct));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await taskService.DeleteAsync(id, ct);
        return NoContent();
    }
}
