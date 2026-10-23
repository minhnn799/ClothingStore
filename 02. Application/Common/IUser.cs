namespace _02._Application.Common;

public interface IUser
{
    Guid? Id { get; }
    string? Role { get; }
}
