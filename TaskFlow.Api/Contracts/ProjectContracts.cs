using System.ComponentModel.DataAnnotations;
using TaskFlow.Api.Models;

namespace TaskFlow.Api.Contracts;

/// <summary>Project details returned by the API.</summary>
public sealed record ProjectResponse(int Id, string Name, string? Description, ProjectStatus Status);

/// <summary>Fields used to create a project.</summary>
public sealed record CreateProjectRequest
{
    [Required, MaxLength(160)]
    public required string Name { get; init; }

    [MaxLength(4000)]
    public string? Description { get; init; }
}

/// <summary>Fields used to update a project.</summary>
public sealed record UpdateProjectRequest
{
    [Required, MaxLength(160)]
    public required string Name { get; init; }

    [MaxLength(4000)]
    public string? Description { get; init; }
}
