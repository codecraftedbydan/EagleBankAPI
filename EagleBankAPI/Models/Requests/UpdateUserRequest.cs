namespace EagleBankAPI.Models.Requests;

public class UpdateUserRequest
{
    public string? Name { get; set; }
    public Address? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}
