using System.ComponentModel.DataAnnotations;

namespace EagleBankAPI.Models.Requests;

public class UpdateUserRequest
{
    public string? Name { get; set; }
    public Address? Address { get; set; }
    [RegularExpression(@"^\+[1-9]\d{1,14}$", ErrorMessage = "phoneNumber must be in E.164 format (e.g. +441234567890)")]
    public string? PhoneNumber { get; set; }

    [EmailAddress(ErrorMessage = "email must be a valid email address")]
    public string? Email { get; set; }
}
