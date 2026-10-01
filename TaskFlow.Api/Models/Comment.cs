namespace TaskFlow.Api.Models;

public class Comment
{
    public int Id { get; set; }
    public required string Content { get; set; }
    public int TaskId { get; set; }
    public WorkItem Task { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
