using _02._Application.Common;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Text.Json;

namespace _01.API.Infrastructure;

public class GlobalExceptionHandler(
    //IServiceScopeFactory scopeFactory,
    ILogger<GlobalExceptionHandler> logger
    ) : IExceptionHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
        )
    {
        //if (httpContext == null || exception == null)
        //{
        //    return false;
        //}

        logger.LogError(
            exception,
            "Unhandled exception at {Path}: {Message}",
            httpContext.Request.Path,
            exception.Message
            );

        // Ghi log vào DB
        //try
        //{
        //    using IServiceScope scope = _scopeFactory.CreateScope();
        //    ILogService logRepo = scope.ServiceProvider.GetRequiredService<ILogService>();
        //    logRepo.Log(
        //        $"[{exception.GetType().Name}]\n" +
        //        $"Path: {httpContext.Request.Path}\n" +
        //        $"Message: {exception.Message}\n" +
        //        $"StackTrace: {exception.StackTrace ?? "No stack trace"}"
        //    );
        //}
        //catch (Exception exception)
        //{
        //    _logger.LogWarning(exception, "Failed to log error to database: {Message}", exception.Message);
        //}

        // Chuẩn bị dữ liệu phản hồi

        var (message, code) = GetErrorCode(exception);

        var response = new
        {
            code,
            isError = true,
            message,
        };

        // Gửi JSON luôn với HTTP 200
        httpContext.Response.StatusCode = (int)HttpStatusCode.OK;
        httpContext.Response.ContentType = "application/json";

        await JsonSerializer.SerializeAsync(
            httpContext.Response.Body,
            response,
            JsonOptions,
            cancellationToken);


        return true;
    }

    private static (string Message, int Code) GetErrorCode(
        Exception exception
        )
    {
        return exception switch
        {
            UserException e =>
                (e.Message ?? "User error occurred.", 400),

            ArgumentException =>
                ("An error occured and has been reported to admin please try again later", 400),

            UnauthorizedAccessException =>
                ("Unauthorized access.", 401),
            InvalidOperationException e
                when e.Message.Contains("Cannot serialize") =>
                    ("Response serialization error.", 500),

            _ => ("Unexpected server error.", 500)
        };
    }
}