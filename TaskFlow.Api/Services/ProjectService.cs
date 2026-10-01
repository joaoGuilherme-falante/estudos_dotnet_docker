using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Contracts;
using TaskFlow.Api.Data;
using TaskFlow.Api.Models;

namespace TaskFlow.Api.Services;

public sealed class ProjectService(TaskFlowDbContext dbContext) : IProjectService
{
    public async Task<IReadOnlyList<ProjectResponse>> GetAllAsync(CancellationToken ct) =>
        await dbContext.Projects.AsNoTracking().OrderBy(project => project.Id)
            .Select(project => ToResponse(project))
            .ToListAsync(ct);

    public async Task<ProjectResponse> GetByIdAsync(int id, CancellationToken ct)
    {
        var project = await dbContext.Projects.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id, ct)
            ?? throw new DomainException("Project was not found.", 404);
        return ToResponse(project);
    }

    public async Task<ProjectResponse> CreateAsync(CreateProjectRequest request, CancellationToken ct)
    {
        var project = new Project { Name = request.Name.Trim(), Description = request.Description?.Trim() };
        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync(ct);
        return ToResponse(project);
    }

    public async Task<ProjectResponse> UpdateAsync(int id, UpdateProjectRequest request, CancellationToken ct)
    {
        var project = await dbContext.Projects.SingleOrDefaultAsync(item => item.Id == id, ct)
            ?? throw new DomainException("Project was not found.", 404);
        if (project.Status == ProjectStatus.Completed)
        {
            throw new DomainException("A completed project cannot be changed.");
        }

        project.Name = request.Name.Trim();
        project.Description = request.Description?.Trim();
        await dbContext.SaveChangesAsync(ct);
        return ToResponse(project);
    }

    public async Task<ProjectResponse> CompleteAsync(int id, CancellationToken ct)
    {
        var project = await dbContext.Projects.Include(item => item.Tasks)
            .SingleOrDefaultAsync(item => item.Id == id, ct)
            ?? throw new DomainException("Project was not found.", 404);
        project.Complete();
        await dbContext.SaveChangesAsync(ct);
        return ToResponse(project);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var project = await dbContext.Projects.SingleOrDefaultAsync(item => item.Id == id, ct)
            ?? throw new DomainException("Project was not found.", 404);
        if (await dbContext.Tasks.AnyAsync(task => task.ProjectId == id, ct))
        {
            throw new DomainException("A project with tasks cannot be deleted.", 409);
        }

        dbContext.Projects.Remove(project);
        await dbContext.SaveChangesAsync(ct);
    }

    private static ProjectResponse ToResponse(Project project) =>
        new(project.Id, project.Name, project.Description, project.Status);
}
