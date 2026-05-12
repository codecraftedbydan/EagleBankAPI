namespace EagleBankAPI.Core.Exceptions;

public class DuplicateEmailException : Exception
{
    public string Email { get; }

    public DuplicateEmailException(string email)
        : base($"Email '{email}' is already registered")
    {
        Email = email;
    }
}
