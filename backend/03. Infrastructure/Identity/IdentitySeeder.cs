using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using _03._Infrastructure.Authentication;
namespace _03._Infrastructure.Identity;
public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var name in new[] { "User", "Admin" })
            if (!await roles.RoleExistsAsync(name))
            {
                var result = await roles.CreateAsync(new IdentityRole(name));
                if (!result.Succeeded) throw new InvalidOperationException("Unable to initialize roles.");
            }
        var config = services.GetRequiredService<IConfiguration>();
        var email = config["SeedAdmin:Email"];
        var password = config["SeedAdmin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await users.FindByEmailAsync(email);
        // Bootstrap only a new account: never elevate an existing public registration.
        if (user is not null) return;
        user = new ApplicationUser { UserName = email, Email = email };
        var created = await users.CreateAsync(user, password);
        if (!created.Succeeded) throw new InvalidOperationException("Admin creation failed: " + string.Join("; ", created.Errors.Select(x => x.Description)));
        var assigned = await users.AddToRolesAsync(user, ["User", "Admin"]);
        if (!assigned.Succeeded) throw new InvalidOperationException("Unable to assign admin roles.");
    }
}
