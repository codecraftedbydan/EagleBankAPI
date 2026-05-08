namespace EagleBankAPI.Models;

public class BadRequestErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public List<ValidationError> Details { get; set; } = new();
}
