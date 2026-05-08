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
[Route("v1/accounts")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IBankAccountService _bankAccountService;

    public AccountsController(IBankAccountService bankAccountService)
    {
        _bankAccountService = bankAccountService;
    }

    private string GetUserIdFromClaims()
    {
        return User.FindFirst("userId")?.Value 
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? string.Empty;
    }

    [HttpPost]
    [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BadRequestErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
    {
        var userId = GetUserIdFromClaims();
        var account = await _bankAccountService.CreateAccountAsync(request.Name, "GBP", userId);
        var response = MapToAccountResponse(account);
        return CreatedAtAction(nameof(GetAccountByNumber), new { accountNumber = response.AccountNumber }, response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(AccountsListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ListAccounts()
    {
        var userId = GetUserIdFromClaims();
        var accounts = await _bankAccountService.GetAccountsByUserIdAsync(userId);
        var response = new AccountsListResponse
        {
            Accounts = accounts.Select(MapToAccountResponse).ToList()
        };
        return Ok(response);
    }

    [HttpGet("{accountNumber}")]
    [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAccountByNumber(string accountNumber)
    {
        var userId = GetUserIdFromClaims();
        var account = await _bankAccountService.GetAccountByNumberAsync(accountNumber, userId);

        if (account == null)
        {
            return NotFound(new ErrorResponse { Message = "Bank account not found" });
        }

        var response = MapToAccountResponse(account);
        return Ok(response);
    }

    [HttpPatch("{accountNumber}")]
    [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadRequestErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateAccount(string accountNumber, [FromBody] UpdateAccountRequest request)
    {
        var userId = GetUserIdFromClaims();
        var account = await _bankAccountService.UpdateAccountAsync(accountNumber, request.Name ?? string.Empty, userId);
        var response = MapToAccountResponse(account);
        return Ok(response);
    }

    [HttpDelete("{accountNumber}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteAccount(string accountNumber)
    {
        var userId = GetUserIdFromClaims();
        await _bankAccountService.DeleteAccountAsync(accountNumber, userId);
        return NoContent();
    }

    private static AccountResponse MapToAccountResponse(BankAccount account)
    {
        return new AccountResponse
        {
            AccountNumber = account.AccountNumber,
            SortCode = account.SortCode,
            Name = account.Name,
            AccountType = account.AccountType,
            Balance = account.Balance,
            Currency = account.Currency.ToString(),
            CreatedTimestamp = account.CreatedTimestamp,
            UpdatedTimestamp = account.UpdatedTimestamp
        };
    }
}
