using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Contracts;
using TaskFlow.Api.Data;
using TaskFlow.Api.Models;
using WorkflowStatus = TaskFlow.Api.Models.TaskStatus;

namespace TaskFlow.Api.Services;

public sealed class TaskService(TaskFlowDbContext dbContext) : ITaskService
{
    public async Task<IReadOnlyList<TaskResponse>> GetAllAsync(int? projectId, CancellationToken ct)
    {
        var query = dbContext.Tasks.AsNoTracking().AsQueryable();
        if (projectId.HasValue)
        {
            query = query.Where(task => task.ProjectId == projectId.Value);
        }

        return await query.OrderBy(task => task.Id).Select(task => ToResponse(task)).ToListAsync(ct);
    }

    public async Task<TaskResponse> GetByIdAsync(int id, CancellationToken ct)
    {
        var task = await dbContext.Tasks.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id, ct)
            ?? throw new DomainException("Task was not found.", 404);
        return ToResponse(task);
    }

    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken ct)
    {
        var project = await dbContext.Projects.SingleOrDefaultAsync(item => item.Id == request.ProjectId, ct)
            ?? throw new DomainException("Project was not found.", 404);
        if (project.Status == ProjectStatus.Completed)
        {
            throw new DomainException("A completed project cannot receive new tasks.");
        }

        await EnsureUserExistsAsync(request.AssignedUserId, ct);
        var task = new WorkItem
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Priority = request.Priority,
            ProjectId = request.ProjectId,
            AssignedUserId = request.AssignedUserId,
            CreatedAt = DateTimeOffset.UtcNow,
            DueDate = request.DueDate?.ToUniversalTime()
        };
        task.ValidateDueDate();
        dbContext.Tasks.Add(task);
        await dbContext.SaveChangesAsync(ct);
        return ToResponse(task);
    }

    public async Task<TaskResponse> UpdateAsync(int id, UpdateTaskRequest request, CancellationToken ct)
    {
        var task = await dbContext.Tasks.SingleOrDefaultAsync(item => item.Id == id, ct)
            ?? throw new DomainException("Task was not found.", 404);
        await EnsureUserExistsAsync(request.AssignedUserId, ct);
        task.Title = request.Title.Trim();
        task.Description = request.Description?.Trim();
        task.Priority = request.Priority;
        task.AssignedUserId = request.AssignedUserId;
        task.DueDate = request.DueDate?.ToUniversalTime();
        task.ValidateDueDate();
        await dbContext.SaveChangesAsync(ct);
        return ToResponse(task);
    }

    public async Task<TaskResponse> ChangeStatusAsync(
        int id,
        WorkflowStatus status,
        int actorId,
        UserRole actorRole,
        CancellationToken ct)
    {
        var task = await dbContext.Tasks.SingleOrDefaultAsync(item => item.Id == id, ct)
            ?? throw new DomainException("Task was not found.", 404);
        task.ChangeStatus(status, actorId, actorRole);
        await dbContext.SaveChangesAsync(ct);
        return ToResponse(task);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var task = await dbContext.Tasks.SingleOrDefaultAsync(item => item.Id == id, ct)
            ?? throw new DomainException("Task was not found.", 404);
        dbContext.Tasks.Remove(task);
        await dbContext.SaveChangesAsync(ct);
    }

    private async Task EnsureUserExistsAsync(int? userId, CancellationToken ct)
    {
        if (userId.HasValue && !await dbContext.Users.AnyAsync(user => user.Id == userId.Value, ct))
        {
            throw new DomainException("Assigned user was not found.", 404);
        }
    }

    private static TaskResponse ToResponse(WorkItem task) =>
        new(task.Id, task.Title, task.Description, task.Status, task.Priority, task.ProjectId,
            task.AssignedUserId, task.CreatedAt, task.DueDate);
}
