using _01.API.Infrastructure;
using _01.API.Infrastructure.Session;
using _02._Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;

namespace _01.API;

public static class DependencyInjection
{
    public static IServiceCollection AddWebServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Jwt");

        services.AddControllers();

        services.AddOpenApi(options =>
        {

            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();

            options.AddOperationTransformer((operation, context, cancellationToken) =>
            {
                var hasAuthorize =
                    context.Description.ActionDescriptor.EndpointMetadata
                        .OfType<IAuthorizeData>()
                        .Any();

                if (hasAuthorize)
                {
                    operation.Security =
                    [
                        new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference(
                        "Bearer",
                        context.Document)] = []
                }
                    ];
                }

                return Task.CompletedTask;
            });
        });

        services.AddHttpContextAccessor();

        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddProblemDetails();

        return services;
    }

    public static IServiceCollection AddIdentityServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IUser, CurrentUser>();
        //services.AddSingleton<IToken, CurrentToken>();

        services.AddWebServices(configuration);

        return services;
    }
}
