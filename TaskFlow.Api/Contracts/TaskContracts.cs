using System.ComponentModel.DataAnnotations;
using TaskFlow.Api.Models;
using WorkflowStatus = TaskFlow.Api.Models.TaskStatus;

namespace TaskFlow.Api.Contracts;

/// <summary>Task details returned by the API.</summary>
public sealed record TaskResponse(
    int Id,
    string Title,
    string? Description,
    WorkflowStatus Status,
    TaskPriority Priority,
    int ProjectId,
    int? AssignedUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? DueDate);

/// <summary>Fields used to create a task in an active project.</summary>
public sealed record CreateTaskRequest
{
    [Required, MaxLength(200)]
    public required string Title { get; init; }

    [MaxLength(4000)]
    public string? Description { get; init; }

    public TaskPriority Priority { get; init; } = TaskPriority.Medium;
    [Range(1, int.MaxValue)]
    public int ProjectId { get; init; }
    [Range(1, int.MaxValue)]
    public int? AssignedUserId { get; init; }
    public DateTimeOffset? DueDate { get; init; }
}

/// <summary>Fields used to update task details without changing its workflow status.</summary>
public sealed record UpdateTaskRequest
{
    [Required, MaxLength(200)]
    public required string Title { get; init; }

    [MaxLength(4000)]
    public string? Description { get; init; }

    public TaskPriority Priority { get; init; }
    [Range(1, int.MaxValue)]
    public int? AssignedUserId { get; init; }
    public DateTimeOffset? DueDate { get; init; }
}

/// <summary>Request to advance a task to its next workflow status.</summary>
public sealed record ChangeTaskStatusRequest
{
    [EnumDataType(typeof(WorkflowStatus))]
    public WorkflowStatus Status { get; init; }
}
