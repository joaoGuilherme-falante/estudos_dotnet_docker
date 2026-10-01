using TaskFlow.Api.Services;

namespace TaskFlow.Api.Models;

public class Project
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public ProjectStatus Status { get; private set; } = ProjectStatus.Active;
    public ICollection<WorkItem> Tasks { get; set; } = new List<WorkItem>();

    public void Complete()
    {
        if (Tasks.Any(task => task.Status != TaskStatus.Done))
        {
            throw new DomainException("A project can only be completed when every task is Done.");
        }

        Status = ProjectStatus.Completed;
    }
}
