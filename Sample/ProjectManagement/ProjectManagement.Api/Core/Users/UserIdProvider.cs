using System.Security.Claims;

namespace ProjectManagement.Api.Core.Users;

public static class UserIdProvider
{
    public static Guid GetUserId(HttpContext httpContext) =>
        Guid.Parse(
            httpContext.User.FindFirstValue("userId") ??
            throw new InvalidOperationException("No User Id was provided!")
        );
}
