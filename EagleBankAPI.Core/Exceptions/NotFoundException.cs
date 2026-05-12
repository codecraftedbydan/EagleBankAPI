namespace EagleBankAPI.Core.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string resourceType, string identifier)
        : base($"{resourceType} with identifier '{identifier}' was not found")
    {
    }
}
