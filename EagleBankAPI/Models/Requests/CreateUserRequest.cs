using System.ComponentModel.DataAnnotations;

namespace EagleBankAPI.Models.Requests;

public class CreateUserRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public Address Address { get; set; } = null!;

    [Required]
    [RegularExpression(@"^\+[1-9]\d{1,14}$", ErrorMessage = "phoneNumber must be in E.164 format (e.g. +441234567890)")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [EmailAddress(ErrorMessage = "email must be a valid email address")]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
