using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Contracts;
using TaskFlow.Api.Data;
using TaskFlow.Api.Models;

namespace TaskFlow.Api.Services;

public sealed class CommentService(TaskFlowDbContext dbContext) : ICommentService
{
    public async Task<IReadOnlyList<CommentResponse>> GetForTaskAsync(int taskId, CancellationToken ct)
    {
        if (!await dbContext.Tasks.AnyAsync(task => task.Id == taskId, ct))
        {
            throw new DomainException("Task was not found.", 404);
        }

        return await dbContext.Comments.AsNoTracking().Where(comment => comment.TaskId == taskId)
            .OrderBy(comment => comment.CreatedAt)
            .Select(comment => new CommentResponse(
                comment.Id, comment.Content, comment.TaskId, comment.UserId, comment.User.Name, comment.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<CommentResponse> CreateAsync(
        int taskId,
        int userId,
        CreateCommentRequest request,
        CancellationToken ct)
    {
        if (!await dbContext.Tasks.AnyAsync(task => task.Id == taskId, ct))
        {
            throw new DomainException("Task was not found.", 404);
        }

        var comment = new Comment
        {
            Content = request.Content.Trim(),
            TaskId = taskId,
            UserId = userId,
            CreatedAt = DateTimeOffset.UtcNow
        };
        dbContext.Comments.Add(comment);
        await dbContext.SaveChangesAsync(ct);
        var authorName = await dbContext.Users.Where(user => user.Id == userId)
            .Select(user => user.Name).SingleAsync(ct);
        return new CommentResponse(
            comment.Id, comment.Content, comment.TaskId, comment.UserId, authorName, comment.CreatedAt);
    }

    public async Task DeleteAsync(int id, int userId, bool isManagerOrAdmin, CancellationToken ct)
    {
        var comment = await dbContext.Comments.SingleOrDefaultAsync(item => item.Id == id, ct)
            ?? throw new DomainException("Comment was not found.", 404);
        if (!isManagerOrAdmin && comment.UserId != userId)
        {
            throw new DomainException("Users can only delete their own comments.", 403);
        }

        dbContext.Comments.Remove(comment);
        await dbContext.SaveChangesAsync(ct);
    }
}
