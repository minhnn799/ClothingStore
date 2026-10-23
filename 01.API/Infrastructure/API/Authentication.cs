using _02._Application.Common;
using Microsoft.AspNetCore.Authorization;

namespace _01.API.Infrastructure.API;

[Authorize]

public class Authentication(IUser user) : ApiResponse
{
    protected readonly IUser user = user;
}