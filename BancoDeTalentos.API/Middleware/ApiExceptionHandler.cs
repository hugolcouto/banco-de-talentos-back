using BancoDeTalentos.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BancoDeTalentos.API.Middleware;

public class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ProblemDetails? details;

        if (exception is NotFoundException)
        {
            details = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not found"
            };
        }
        else
        {
            details = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = exception.Message
            };
        };

        httpContext.Response.StatusCode = details.Status ?? StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(details, cancellationToken);
        
        return true;
    }
}