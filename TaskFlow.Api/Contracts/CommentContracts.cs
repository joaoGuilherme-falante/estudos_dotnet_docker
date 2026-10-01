using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.Contracts;

/// <summary>Comment details returned by the API.</summary>
public sealed record CommentResponse(
    int Id,
    string Content,
    int TaskId,
    int UserId,
    string AuthorName,
    DateTimeOffset CreatedAt);

/// <summary>Content for a comment on a task.</summary>
public sealed record CreateCommentRequest
{
    [Required, MaxLength(4000)]
    public required string Content { get; init; }
}
