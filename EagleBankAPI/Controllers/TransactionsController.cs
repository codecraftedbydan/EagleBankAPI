using EagleBankAPI.Core.Entities;
using EagleBankAPI.Core.Services.Interfaces;
using EagleBankAPI.Models;
using EagleBankAPI.Models.Requests;
using EagleBankAPI.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        var userId = GetUserIdFromClaims();
        var transaction = await _transactionService.CreateTransactionAsync(accountNumber, request.Amount, request.Currency, request.Type, request.Reference, userId);
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
