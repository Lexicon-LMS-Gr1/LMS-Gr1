namespace Domain.Models.Exceptions;

public class TokenValidationException : DomainException
{
    public TokenValidationException(string message = "Invalid or expired token", int statusCode = 401)
        : base(message, "Unauthorized", statusCode)
    {
    }
}
