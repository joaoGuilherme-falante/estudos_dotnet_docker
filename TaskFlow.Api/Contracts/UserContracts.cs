using System.ComponentModel.DataAnnotations;
using TaskFlow.Api.Models;

namespace TaskFlow.Api.Contracts;

/// <summary>Public user data; password hashes are never exposed.</summary>
public sealed record UserResponse(int Id, string Name, string Email, UserRole Role);

/// <summary>Data required to create a user account.</summary>
public sealed record CreateUserRequest
{
    [Required, MaxLength(120)]
    public required string Name { get; init; }

    [Required, EmailAddress, MaxLength(320)]
    public required string Email { get; init; }

    [Required, MinLength(12), MaxLength(128)]
    public required string Password { get; init; }

    [EnumDataType(typeof(UserRole))]
    public UserRole Role { get; init; } = UserRole.Developer;
}

/// <summary>Fields an administrator can change on a user account.</summary>
public sealed record UpdateUserRequest
{
    [Required, MaxLength(120)]
    public required string Name { get; init; }

    [Required, EmailAddress, MaxLength(320)]
    public required string Email { get; init; }

    [EnumDataType(typeof(UserRole))]
    public UserRole Role { get; init; }

    [MinLength(12), MaxLength(128)]
    public string? NewPassword { get; init; }
}
