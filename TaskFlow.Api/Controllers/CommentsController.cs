using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Contracts;
using TaskFlow.Api.Models;
using TaskFlow.Api.Services;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Authorize]
public sealed class CommentsController(ICommentService commentService) : ControllerBase
{
    [HttpGet("api/tasks/{taskId:int}/comments")]
    public async Task<ActionResult<IReadOnlyList<CommentResponse>>> GetForTask(
        int taskId,
        CancellationToken ct) =>
        Ok(await commentService.GetForTaskAsync(taskId, ct));

    [HttpPost("api/tasks/{taskId:int}/comments")]
    public async Task<ActionResult<CommentResponse>> Create(
        int taskId,
        CreateCommentRequest request,
        CancellationToken ct)
    {
        var comment = await commentService.CreateAsync(taskId, CurrentUser.Id(User), request, ct);
        return CreatedAtAction(nameof(GetForTask), new { taskId }, comment);
    }

    [HttpDelete("api/comments/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var roleClaim = User.FindFirst("role")?.Value;
        var canModerate = roleClaim is nameof(UserRole.Admin) or nameof(UserRole.Manager);
        await commentService.DeleteAsync(id, CurrentUser.Id(User), canModerate, ct);
        return NoContent();
    }
}
