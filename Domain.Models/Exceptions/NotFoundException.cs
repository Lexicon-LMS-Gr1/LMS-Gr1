namespace Domain.Models.Exceptions;

public class NotFoundException : DomainException
{
    public NotFoundException(string message, string title = "Not Found") 
        : base(message, title, 404)
    {
    }
}
