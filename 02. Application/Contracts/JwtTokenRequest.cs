using System;
using System.Collections.Generic;
using System.Text;

namespace _02._Application.Contracts;

public sealed record JwtTokenRequest(
    string UserId,
    string Email,
    IEnumerable<string> Roles
    );