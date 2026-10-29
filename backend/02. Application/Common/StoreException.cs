namespace _02._Application.Common;
public sealed class StoreException(string message, int statusCode = 400) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
