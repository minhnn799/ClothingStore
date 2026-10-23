namespace _02._Application.Contracts;

public sealed record TokenResponse(
    string AccessToken,
    string RefreshToken
    );
