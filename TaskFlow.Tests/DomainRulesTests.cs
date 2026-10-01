using TaskFlow.Api.Models;
using TaskFlow.Api.Services;
using WorkflowStatus = TaskFlow.Api.Models.TaskStatus;

namespace TaskFlow.Tests;

public sealed class DomainRulesTests
{
    [Fact]
    public void TaskMustAdvanceOneStatusAtATime()
    {
        var task = CreateAssignedTask();

        task.ChangeStatus(WorkflowStatus.Todo, actorUserId: 7, UserRole.Developer);

        Assert.Equal(WorkflowStatus.Todo, task.Status);
        Assert.Throws<DomainException>(() =>
            task.ChangeStatus(WorkflowStatus.Review, actorUserId: 7, UserRole.Developer));
    }

    [Fact]
    public void OnlyAssigneeOrManagerOrAdminCanChangeTaskStatus()
    {
        var task = CreateAssignedTask();

        var exception = Assert.Throws<DomainException>(() =>
            task.ChangeStatus(WorkflowStatus.Todo, actorUserId: 8, UserRole.Developer));

        Assert.Equal(403, exception.StatusCode);
        task.ChangeStatus(WorkflowStatus.Todo, actorUserId: 8, UserRole.Manager);
        task.ChangeStatus(WorkflowStatus.InProgress, actorUserId: 9, UserRole.Admin);
    }

    [Fact]
    public void ProjectCannotCompleteWhileAnyTaskIsNotDone()
    {
        var project = new Project { Name = "Project" };
        project.Tasks.Add(CreateAssignedTask());

        Assert.Throws<DomainException>(project.Complete);
        Assert.Equal(ProjectStatus.Active, project.Status);
    }

    [Fact]
    public void ProjectCanCompleteWhenAllTasksAreDone()
    {
        var task = CreateAssignedTask();
        foreach (var status in Enum.GetValues<WorkflowStatus>().Skip(1))
        {
            task.ChangeStatus(status, actorUserId: 7, UserRole.Developer);
        }

        var project = new Project { Name = "Project" };
        project.Tasks.Add(task);
        project.Complete();

        Assert.Equal(ProjectStatus.Completed, project.Status);
    }

    [Fact]
    public void DueDateCannotBeEarlierThanCreationDate()
    {
        var task = CreateAssignedTask();
        task.CreatedAt = new DateTimeOffset(2026, 10, 2, 0, 0, 0, TimeSpan.Zero);
        task.DueDate = new DateTimeOffset(2026, 10, 1, 23, 0, 0, TimeSpan.Zero);

        Assert.Throws<DomainException>(task.ValidateDueDate);
    }

    private static WorkItem CreateAssignedTask() => new()
    {
        Title = "Task",
        ProjectId = 1,
        AssignedUserId = 7
    };
}
