using TaskFlow.Api.Contracts;

namespace TaskFlow.Api.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}
