namespace EagleBankAPI.Core.Exceptions;

public class UserHasAccountsException : Exception
{
    public string UserId { get; }
    public int AccountCount { get; }

    public UserHasAccountsException(string userId, int accountCount) 
        : base($"Cannot delete user '{userId}' because they have {accountCount} active bank account(s)")
    {
        UserId = userId;
        AccountCount = accountCount;
    }
}
