using TaskFlow.Api.Contracts;
using TaskFlow.Api.Models;
using WorkflowStatus = TaskFlow.Api.Models.TaskStatus;

namespace TaskFlow.Api.Services;

public interface ITaskService
{
    Task<IReadOnlyList<TaskResponse>> GetAllAsync(int? projectId, CancellationToken ct);
    Task<TaskResponse> GetByIdAsync(int id, CancellationToken ct);
    Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken ct);
    Task<TaskResponse> UpdateAsync(int id, UpdateTaskRequest request, CancellationToken ct);
    Task<TaskResponse> ChangeStatusAsync(int id, WorkflowStatus status, int actorId, UserRole actorRole, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
}
