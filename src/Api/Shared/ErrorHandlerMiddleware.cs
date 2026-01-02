using System.Net;
using System.Text.Json;
using FluentValidation;

namespace AI.PurchaseService.Shared
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
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
            catch (Exception error)
            {
                _logger.LogError(error, "An unhandled exception occurred");
                
                var response = context.Response;
                response.ContentType = "application/json";

                object result = error switch
                {
                    ValidationException validationException => new
                    {
                        message = "Validation failed",
                        errors = validationException.Errors.Select(e => new
                        {
                            field = e.PropertyName,
                            message = e.ErrorMessage
                        })
                    },
                    ArgumentException => new { message = error.Message },
                    _ => new { message = "An error occurred while processing your request" }
                };

                response.StatusCode = error switch
                {
                    ValidationException => (int)HttpStatusCode.BadRequest,
                    ArgumentException => (int)HttpStatusCode.BadRequest,
                    _ => (int)HttpStatusCode.InternalServerError
                };

                await response.WriteAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                }));
            }
        }
    }
}