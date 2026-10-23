using _02._Application.Common;
using System.Security.Claims;

namespace _01.API.Infrastructure.Session;

public class CurrentToken(IHttpContextAccessor httpContextAccessor) : IToken
{
    public string? Id => httpContextAccessor.HttpContext?.User.FindFirstValue("jti");

    public string? Expire => httpContextAccessor.HttpContext?.User.FindFirstValue("exp");
}