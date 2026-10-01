using TaskFlow.Api.Services;

namespace TaskFlow.Api.Models;

public class WorkItem
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public TaskStatus Status { get; private set; } = TaskStatus.Backlog;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public int? AssignedUserId { get; set; }
    public User? AssignedUser { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DueDate { get; set; }
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public void ChangeStatus(TaskStatus newStatus, int actorUserId, UserRole actorRole)
    {
        if (actorRole is not (UserRole.Admin or UserRole.Manager) && AssignedUserId != actorUserId)
        {
            throw new DomainException("Only the assigned user, a Manager, or an Admin can change task status.", 403);
        }

        if ((int)newStatus != (int)Status + 1)
        {
            throw new DomainException($"Task status must advance one step from {Status}.");
        }

        Status = newStatus;
    }

    public void ValidateDueDate()
    {
        if (DueDate is not null && DueDate.Value.UtcDateTime.Date < CreatedAt.UtcDateTime.Date)
        {
            throw new DomainException("Due date cannot be earlier than the task creation date.");
        }
    }
}
