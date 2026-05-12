using EagleBankAPI.Core.Entities;
using EagleBankAPI.Core.Services.Interfaces;
using EagleBankAPI.Models;
using EagleBankAPI.Models.Requests;
using EagleBankAPI.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace EagleBankAPI.Controllers;

[ApiController]
[Route("v1/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    private string GetUserIdFromClaims()
    {
        return User.FindFirst("userId")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? string.Empty;
    }

    private static bool IsValidUserId(string userId)
        => Regex.IsMatch(userId, @"^usr-[A-Za-z0-9]+$");

    private BadRequestObjectResult InvalidUserId() =>
        BadRequest(new BadRequestErrorResponse
        {
            Message = "Invalid user ID format",
            Details = [new ValidationError { Field = "userId", Message = "Must match pattern ^usr-[A-Za-z0-9]+$", Type = "validation_error" }]
        });

    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BadRequestErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var user = await _userService.CreateUserAsync(
            request.Name,
            request.Email,
            request.Password,
            request.PhoneNumber,
            request.Address.Line1,
            request.Address.Line2,
            request.Address.Line3,
            request.Address.Town,
            request.Address.County,
            request.Address.Postcode
        );

        var response = MapToUserResponse(user);
        return CreatedAtAction(nameof(GetUserById), new { userId = response.Id }, response);
    }

    [HttpGet("{userId}")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadRequestErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserById(string userId)
    {
        if (!IsValidUserId(userId)) return InvalidUserId();
        var requestingUserId = GetUserIdFromClaims();
        var user = await _userService.GetUserByIdAsync(userId, requestingUserId);

        if (user == null)
        {
            return NotFound(new ErrorResponse { Message = "User not found" });
        }

        var response = MapToUserResponse(user);
        return Ok(response);
    }

    [HttpPatch("{userId}")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadRequestErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserRequest request)
    {
        if (!IsValidUserId(userId)) return InvalidUserId();
        var requestingUserId = GetUserIdFromClaims();
        var user = await _userService.UpdateUserAsync(
            userId,
            request.Name,
            request.Email,
            request.PhoneNumber,
            request.Address?.Line1,
            request.Address?.Line2,
            request.Address?.Line3,
            request.Address?.Town,
            request.Address?.County,
            request.Address?.Postcode,
            requestingUserId
        );

        var response = MapToUserResponse(user);
        return Ok(response);
    }

    [HttpDelete("{userId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(BadRequestErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        if (!IsValidUserId(userId)) return InvalidUserId();
        var requestingUserId = GetUserIdFromClaims();
        await _userService.DeleteUserAsync(userId, requestingUserId);
        return NoContent();
    }

    private static UserResponse MapToUserResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Address = new Address
            {
                Line1 = user.AddressLine1,
                Line2 = user.AddressLine2,
                Line3 = user.AddressLine3,
                Town = user.AddressTown,
                County = user.AddressCounty,
                Postcode = user.AddressPostcode
            },
            CreatedTimestamp = user.CreatedTimestamp,
            UpdatedTimestamp = user.UpdatedTimestamp
        };
    }
}
