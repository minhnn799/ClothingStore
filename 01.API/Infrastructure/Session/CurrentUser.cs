using _02._Application.Common;
using System.Security.Claims;

namespace _01.API.Infrastructure.Session;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : IUser
{
    public Guid? Id
    {
        get
        {
            string? value =
                httpContextAccessor.HttpContext?.User
                    ?.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out Guid id)
                ? id
                : null;
        }
    }

    public string? Role
    {
        get
        {
            return httpContextAccessor.HttpContext?.User
                ?.FindFirstValue(ClaimTypes.Role);
        }
    }
}
