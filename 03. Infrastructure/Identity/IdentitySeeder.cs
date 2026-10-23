using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace _03._Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles =
        [
            "User",
            "Admin"
        ];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(
                    new IdentityRole(role));
            }
        }
    }
}
