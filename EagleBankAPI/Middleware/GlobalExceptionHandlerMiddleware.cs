using EagleBankAPI.Core.Exceptions;
using EagleBankAPI.Models;
using System.Net;
using System.Text.Json;

namespace EagleBankAPI.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            NotFoundException notFound => (HttpStatusCode.NotFound, notFound.Message),
            ForbiddenException forbidden => (HttpStatusCode.Forbidden, forbidden.Message),
            InsufficientFundsException insufficientFunds => (HttpStatusCode.UnprocessableEntity, insufficientFunds.Message),
            DuplicateEmailException duplicate => (HttpStatusCode.Conflict, duplicate.Message),
            UserHasAccountsException userHasAccounts => (HttpStatusCode.Conflict, userHasAccounts.Message),
            InvalidTransactionException invalidTransaction => (HttpStatusCode.BadRequest, invalidTransaction.Message),
            UnauthorizedAccessException unauthorized => (HttpStatusCode.Forbidden, unauthorized.Message),
            ArgumentException argument => (HttpStatusCode.BadRequest, argument.Message),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var errorResponse = new ErrorResponse
        {
            Message = message
        };

        var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return context.Response.WriteAsync(json);
    }
}
