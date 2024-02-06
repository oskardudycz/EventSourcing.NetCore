using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Core.WebApi.Middlewares.ExceptionHandling;

public static class ExceptionHandlingMiddleware
{
    public static IApplicationBuilder UseExceptionHandlingMiddleware(
        this IApplicationBuilder app,
        Func<Exception, HttpContext, ProblemDetails?>? customExceptionMap = null
    ) =>
        app.UseExceptionHandler(exceptionHandlerApp =>
        {
            exceptionHandlerApp.Run(async context =>
            {
                context.Response.ContentType = "application/problem+json";

                if (context.RequestServices.GetService<IProblemDetailsService>() is not { } problemDetailsService)
                    return;

                var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                var exception = exceptionHandlerFeature?.Error;

                if (exception is null)
                    return;
                
                var details = customExceptionMap?.Invoke(exception, context) ??
                              MapExceptionUsingDefaults(exception);

                var problem = new ProblemDetailsContext { HttpContext = context, ProblemDetails = details };

                problem.ProblemDetails.Extensions.Add("exception", exceptionHandlerFeature?.Error.ToString());

                await problemDetailsService.WriteAsync(problem).ConfigureAwait(false);
            });
        });

    private static ProblemDetails MapExceptionUsingDefaults(Exception exception)
    {
        var statusCode = exception switch
        {
            ArgumentException _ => StatusCodes.Status400BadRequest,
            ValidationException _ => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException _ => StatusCodes.Status401Unauthorized,
            InvalidOperationException _ => StatusCodes.Status403Forbidden,
            NotImplementedException _ => StatusCodes.Status501NotImplemented,
            _ => StatusCodes.Status500InternalServerError
        };

        return exception.MapToProblemDetails(statusCode);
    }
}

public static class ProblemDetailsExtensions
{
    public static ProblemDetails MapToProblemDetails(
        this Exception exception,
        int statusCode,
        string? title = null,
        string? detail = null
    ) =>
        new() { Title = title ?? exception.GetType().Name, Detail = detail ?? exception.Message, Status = statusCode };
}
