using TaskFlow.Api.Contracts;

namespace TaskFlow.Api.Services;

public interface ICommentService
{
    Task<IReadOnlyList<CommentResponse>> GetForTaskAsync(int taskId, CancellationToken ct);
    Task<CommentResponse> CreateAsync(int taskId, int userId, CreateCommentRequest request, CancellationToken ct);
    Task DeleteAsync(int id, int userId, bool isManagerOrAdmin, CancellationToken ct);
}
