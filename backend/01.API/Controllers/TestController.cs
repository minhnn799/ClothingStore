using _01.API.Infrastructure.API;
using _02._Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace _01.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController(IUser user) : Authentication(user)

{
    [HttpGet("protected")]
    public IActionResult Protected()
    {
        return OkResponse(user);
    }

    [Authorize(Roles = "User")]
    [HttpGet("user")]
    public IActionResult UserOnly()
    {
        return OkResponse("User role accepted.");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public IActionResult AdminOnly()
    {
        return OkResponse("Admin role accepted.");
    }
}