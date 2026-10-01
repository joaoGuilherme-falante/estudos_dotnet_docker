using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Contracts;
using TaskFlow.Api.Services;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
public sealed class ProjectsController(IProjectService projectService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjectResponse>>> GetAll(CancellationToken ct) =>
        Ok(await projectService.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjectResponse>> GetById(int id, CancellationToken ct) =>
        Ok(await projectService.GetByIdAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ProjectResponse>> Create(CreateProjectRequest request, CancellationToken ct)
    {
        var project = await projectService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ProjectResponse>> Update(
        int id,
        UpdateProjectRequest request,
        CancellationToken ct) =>
        Ok(await projectService.UpdateAsync(id, request, ct));

    [HttpPost("{id:int}/complete")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ProjectResponse>> Complete(int id, CancellationToken ct) =>
        Ok(await projectService.CompleteAsync(id, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await projectService.DeleteAsync(id, ct);
        return NoContent();
    }
}
