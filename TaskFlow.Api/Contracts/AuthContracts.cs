using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.Contracts;

/// <summary>Credentials used to obtain a JWT access token.</summary>
public sealed record LoginRequest
{
    [Required, EmailAddress, MaxLength(320)]
    public required string Email { get; init; }

    [Required, MinLength(1)]
    public required string Password { get; init; }
}

/// <summary>JWT access token returned after successful authentication.</summary>
public sealed record LoginResponse(string AccessToken, DateTimeOffset ExpiresAt);
