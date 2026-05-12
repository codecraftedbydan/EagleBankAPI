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
        context.Response.ContentType = "application/json";
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        // InvalidTransactionException and ArgumentException are 400s that must return BadRequestErrorResponse
        if (exception is InvalidTransactionException or ArgumentException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            var badRequestResponse = new BadRequestErrorResponse
            {
                Message = exception.Message,
                Details = []
            };
            return context.Response.WriteAsync(JsonSerializer.Serialize(badRequestResponse, jsonOptions));
        }

        var (statusCode, message) = exception switch
        {
            NotFoundException notFound => (HttpStatusCode.NotFound, notFound.Message),
            UnauthorizedException unauthorized => (HttpStatusCode.Unauthorized, unauthorized.Message),
            ForbiddenException forbidden => (HttpStatusCode.Forbidden, forbidden.Message),
            InsufficientFundsException insufficientFunds => (HttpStatusCode.UnprocessableEntity, insufficientFunds.Message),
            DuplicateEmailException duplicate => (HttpStatusCode.Conflict, duplicate.Message),
            UserHasAccountsException userHasAccounts => (HttpStatusCode.Conflict, userHasAccounts.Message),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
        };

        context.Response.StatusCode = (int)statusCode;

        var errorResponse = new ErrorResponse
        {
            Message = message
        };

        var json = JsonSerializer.Serialize(errorResponse, jsonOptions);

        return context.Response.WriteAsync(json);
    }
}
