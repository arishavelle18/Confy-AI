using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Diagnostics;

namespace ConfyAI.API.Behavior;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var failures = new List<FluentValidation.Results.ValidationFailure>();

        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            failures.AddRange(result.Errors);
        }

        if (failures.Count > 0)
            throw new ValidationException(failures);

        return await next(cancellationToken);
    }
}

internal class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var (status, message) = exception switch
        {
            ValidationException v => (StatusCodes.Status400BadRequest, v.Errors.First().ErrorMessage),
            HttpRequestException => (StatusCodes.Status503ServiceUnavailable,
                "Cannot reach Ollama. Make sure it is running and the model is installed."),
            BadHttpRequestException b => (b.StatusCode, "The request could not be read. Check the request body and Content-Type."),
            _ => (StatusCodes.Status500InternalServerError, "Something went wrong while checking the contract.")
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception");

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new { error = message }, cancellationToken);
        return true;
    }
}