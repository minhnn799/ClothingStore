using Microsoft.AspNetCore.Mvc;

namespace _01.API.Infrastructure.API;

[ApiController]
public abstract class ApiResponse : ControllerBase
{
    protected IActionResult OkResponse(object data)
    {
        return Ok(new { status = 200, returnObject = data, isSuccess = true });
    }

    protected IActionResult BadResponse(object data)
    {
        return Ok(new { status = 400, returnObject = data, isSuccess = true });
    }
}
