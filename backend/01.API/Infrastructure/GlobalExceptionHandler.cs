using _02._Application.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Npgsql;
namespace _01.API.Infrastructure;
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private static bool IsSerializationConflict(Exception exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
            if (current is PostgresException { SqlState: "40001" or "40P01" }) return true;
        return false;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var (code, message) = exception switch
        {
            StoreException e => (e.StatusCode, e.Message),
            UserException e => (400, e.Message),
            UnauthorizedAccessException => (401, "Invalid credentials or expired session."),
            DbUpdateConcurrencyException => (409, "The resource changed. Reload and retry."),
            PostgresException e when e.SqlState is "40001" or "40P01" => (409, "Concurrent update. Reload and retry."),
            DbUpdateException e when e.InnerException is PostgresException p && (p.SqlState is "23505" or "23503" or "40001" or "40P01") => (409, "Conflicting update or resource already in use. Reload and retry."),
            _ when IsSerializationConflict(exception) => (409, "Concurrent update. Reload and retry."),
            _ => (500, "Unexpected server error.")
        };
        if (code >= 500) logger.LogError(exception, "Request failed at {Path}", context.Request.Path);
        context.Response.StatusCode = code;
        await context.Response.WriteAsJsonAsync(new { code, isError = true, message }, ct);
        return true;
    }
}
