namespace EagleBankAPI.Models.Responses;

public class UserResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Address Address { get; set; } = null!;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedTimestamp { get; set; }
    public DateTime UpdatedTimestamp { get; set; }
}
