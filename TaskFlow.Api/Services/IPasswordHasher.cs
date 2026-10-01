using TaskFlow.Api.Models;

namespace TaskFlow.Api.Services;

public interface IPasswordHasher
{
    string Hash(User user, string password);
    bool Verify(User user, string password);
}
