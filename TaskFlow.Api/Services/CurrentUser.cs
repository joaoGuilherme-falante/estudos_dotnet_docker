using System.Security.Claims;

namespace TaskFlow.Api.Services;

public static class CurrentUser
{
    public static int Id(ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id)
            ? id
            : throw new DomainException("The authenticated token has no valid user identifier.", 401);
    }
}
