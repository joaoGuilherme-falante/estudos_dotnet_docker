using TaskFlow.Api.Contracts;

namespace TaskFlow.Api.Services;

public interface IProjectService
{
    Task<IReadOnlyList<ProjectResponse>> GetAllAsync(CancellationToken ct);
    Task<ProjectResponse> GetByIdAsync(int id, CancellationToken ct);
    Task<ProjectResponse> CreateAsync(CreateProjectRequest request, CancellationToken ct);
    Task<ProjectResponse> UpdateAsync(int id, UpdateProjectRequest request, CancellationToken ct);
    Task<ProjectResponse> CompleteAsync(int id, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
}
