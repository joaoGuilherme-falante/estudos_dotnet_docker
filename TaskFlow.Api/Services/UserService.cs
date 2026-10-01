using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Contracts;
using TaskFlow.Api.Data;
using TaskFlow.Api.Models;

namespace TaskFlow.Api.Services;

public sealed class UserService(TaskFlowDbContext dbContext, IPasswordHasher passwordHasher) : IUserService
{
    public async Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken ct) =>
        await dbContext.Users.AsNoTracking().OrderBy(user => user.Id)
            .Select(user => new UserResponse(user.Id, user.Name, user.Email, user.Role))
            .ToListAsync(ct);

    public async Task<UserResponse> GetByIdAsync(int id, CancellationToken ct)
    {
        var user = await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id, ct)
            ?? throw new DomainException("User was not found.", 404);
        return ToResponse(user);
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken ct)
    {
        var email = NormalizeEmail(request.Email);
        if (await dbContext.Users.AnyAsync(user => user.Email == email, ct))
        {
            throw new DomainException("A user with this email already exists.", 409);
        }

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = email,
            PasswordHash = string.Empty,
            Role = request.Role
        };
        user.PasswordHash = passwordHasher.Hash(user, request.Password);
        dbContext.Users.Add(user);
        await SaveAsync(ct);
        return ToResponse(user);
    }

    public async Task<UserResponse> UpdateAsync(int id, UpdateUserRequest request, CancellationToken ct)
    {
        var user = await dbContext.Users.SingleOrDefaultAsync(item => item.Id == id, ct)
            ?? throw new DomainException("User was not found.", 404);
        var email = NormalizeEmail(request.Email);
        if (await dbContext.Users.AnyAsync(item => item.Id != id && item.Email == email, ct))
        {
            throw new DomainException("A user with this email already exists.", 409);
        }

        user.Name = request.Name.Trim();
        user.Email = email;
        user.Role = request.Role;
        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            user.PasswordHash = passwordHasher.Hash(user, request.NewPassword);
        }

        await SaveAsync(ct);
        return ToResponse(user);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var user = await dbContext.Users.SingleOrDefaultAsync(item => item.Id == id, ct)
            ?? throw new DomainException("User was not found.", 404);
        if (await dbContext.Tasks.AnyAsync(task => task.AssignedUserId == id, ct) ||
            await dbContext.Comments.AnyAsync(comment => comment.UserId == id, ct))
        {
            throw new DomainException("User cannot be deleted while they are assigned tasks or have comments.", 409);
        }

        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync(ct);
    }

    private async Task SaveAsync(CancellationToken ct)
    {
        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException exception) when (exception.InnerException is Npgsql.PostgresException
        { SqlState: Npgsql.PostgresErrorCodes.UniqueViolation })
        {
            throw new DomainException("A user with this email already exists.", 409);
        }
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
    private static UserResponse ToResponse(User user) => new(user.Id, user.Name, user.Email, user.Role);
}
