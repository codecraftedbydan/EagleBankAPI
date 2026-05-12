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
[Route("v1/accounts/{accountNumber}/transactions")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    private string GetUserIdFromClaims()
    {
        return User.FindFirst("userId")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? string.Empty;
    }

    private static bool IsValidAccountNumber(string accountNumber)
        => Regex.IsMatch(accountNumber, @"^01\d{6}$");

    private static bool IsValidTransactionId(string transactionId)
        => Regex.IsMatch(transactionId, @"^tan-[A-Za-z0-9]+$");

    private BadRequestObjectResult InvalidAccountNumber() =>
        BadRequest(new BadRequestErrorResponse
        {
            Message = "Invalid account number format",
            Details = [new ValidationError { Field = "accountNumber", Message = "Must match pattern ^01\\d{6}$", Type = "validation_error" }]
        });

    private BadRequestObjectResult InvalidTransactionId() =>
        BadRequest(new BadRequestErrorResponse
        {
            Message = "Invalid transaction ID format",
            Details = [new ValidationError { Field = "transactionId", Message = "Must match pattern ^tan-[A-Za-z0-9]+$", Type = "validation_error" }]
        });

    [HttpPost]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BadRequestErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateTransaction(string accountNumber, [FromBody] CreateTransactionRequest request)
    {
        if (!IsValidAccountNumber(accountNumber)) return InvalidAccountNumber();
        var userId = GetUserIdFromClaims();
        var transaction = await _transactionService.CreateTransactionAsync(accountNumber, request.Amount!.Value, request.Currency, request.Type, request.Reference, userId);
        var response = MapToTransactionResponse(transaction);
        return CreatedAtAction(nameof(GetTransactionById), new { accountNumber, transactionId = response.Id }, response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(TransactionsListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadRequestErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ListTransactions(string accountNumber)
    {
        if (!IsValidAccountNumber(accountNumber)) return InvalidAccountNumber();
        var userId = GetUserIdFromClaims();
        var transactions = await _transactionService.GetTransactionsByAccountNumberAsync(accountNumber, userId);
        var response = new TransactionsListResponse
        {
            Transactions = transactions.Select(MapToTransactionResponse).ToList()
        };
        return Ok(response);
    }

    [HttpGet("{transactionId}")]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadRequestErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTransactionById(string accountNumber, string transactionId)
    {
        if (!IsValidAccountNumber(accountNumber)) return InvalidAccountNumber();
        if (!IsValidTransactionId(transactionId)) return InvalidTransactionId();
        var userId = GetUserIdFromClaims();
        var transaction = await _transactionService.GetTransactionByIdAsync(accountNumber, transactionId, userId);

        if (transaction == null)
        {
            return NotFound(new ErrorResponse { Message = "Transaction not found" });
        }

        var response = MapToTransactionResponse(transaction);
        return Ok(response);
    }

    private static TransactionResponse MapToTransactionResponse(Transaction transaction)
    {
        return new TransactionResponse
        {
            Id = transaction.Id,
            Amount = transaction.Amount,
            Currency = transaction.Currency.ToString(),
            Type = transaction.Type.ToString().ToLower(),
            Reference = transaction.Reference,
            UserId = transaction.UserId,
            CreatedTimestamp = transaction.CreatedTimestamp
        };
    }
}
