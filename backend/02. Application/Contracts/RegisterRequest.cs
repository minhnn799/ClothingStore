using System.ComponentModel.DataAnnotations;

namespace _02._Application.Contracts;

public sealed record RegisterRequest(
    [Required, EmailAddress]
    string Email,

    [Required, MinLength(8)]
    string Password
);
