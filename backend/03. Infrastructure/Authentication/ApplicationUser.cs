using Microsoft.AspNetCore.Identity;

namespace _03._Infrastructure.Authentication;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
