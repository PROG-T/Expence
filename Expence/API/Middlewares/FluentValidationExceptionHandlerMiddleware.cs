/*using FluentValidation;
using Serilog;
using System.Net;

namespace Expence.API.Middlewares
{
    public class FluentValidationExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public FluentValidationExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException validationException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";

                var errors = validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                var response = new
                {
                    status = false,
                    message = "One or more validation errors occurred",
                    errors = errors
                };

                Log.Warning("Validation error: {@ValidationErrors}", errors);
                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }

}
*/