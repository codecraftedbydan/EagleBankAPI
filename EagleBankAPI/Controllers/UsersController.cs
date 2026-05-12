using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EagleBankAPI.Models;
using EagleBankAPI.Models.Requests;
using EagleBankAPI.Models.Responses;
using EagleBankAPI.Core.Services.Interfaces;
using EagleBankAPI.Core.Entities;

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

    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BadRequestErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new BadRequestErrorResponse
            {
                Message = "Invalid request data",
                Details = ModelState.Select(ms => new ValidationError
                {
                    Field = ms.Key,
                    Message = string.Join(", ", ms.Value?.Errors.Select(e => e.ErrorMessage) ?? Array.Empty<string>()),
                    Type = "validation_error"
                }).ToList()
            });
        }

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
        var requestingUserId = GetUserIdFromClaims();
        var user = await _userService.UpdateUserAsync(
            userId, 
            request.Name ?? string.Empty, 
            request.Email ?? string.Empty, 
            request.PhoneNumber ?? string.Empty,
            request.Address?.Line1 ?? string.Empty, 
            request.Address?.Line2, 
            request.Address?.Line3, 
            request.Address?.Town ?? string.Empty, 
            request.Address?.County ?? string.Empty, 
            request.Address?.Postcode ?? string.Empty,
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
