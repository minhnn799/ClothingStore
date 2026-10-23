using System.ComponentModel.DataAnnotations;

namespace _02._Application.Contracts;

public sealed record LoginRequest(
    [Required, EmailAddress]
    string Email,

    [Required]
    string Password
);
