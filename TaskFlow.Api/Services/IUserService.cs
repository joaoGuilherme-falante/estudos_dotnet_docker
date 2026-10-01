using TaskFlow.Api.Contracts;

namespace TaskFlow.Api.Services;

public interface IUserService
{
    Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken ct);
    Task<UserResponse> GetByIdAsync(int id, CancellationToken ct);
    Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken ct);
    Task<UserResponse> UpdateAsync(int id, UpdateUserRequest request, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
}
