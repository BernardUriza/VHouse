using System.Net;
using System.Text.Json;

namespace VHouse.Middleware
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
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
                _logger.LogError(ex, "An unhandled exception occurred. Request: {Method} {Path}", 
                    context.Request.Method, context.Request.Path);

                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var response = new
            {
                error = new
                {
                    message = "An internal server error occurred.",
                    details = GetExceptionDetails(exception),
                    timestamp = DateTime.UtcNow,
                    requestId = context.TraceIdentifier
                }
            };

            switch (exception)
            {
                case ArgumentException:
                case ArgumentNullException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = new
                    {
                        error = new
                        {
                            message = "Invalid request parameters.",
                            details = exception.Message,
                            timestamp = DateTime.UtcNow,
                            requestId = context.TraceIdentifier
                        }
                    };
                    break;

                case UnauthorizedAccessException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response = new
                    {
                        error = new
                        {
                            message = "Unauthorized access.",
                            details = "You do not have permission to access this resource.",
                            timestamp = DateTime.UtcNow,
                            requestId = context.TraceIdentifier
                        }
                    };
                    break;

                case KeyNotFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response = new
                    {
                        error = new
                        {
                            message = "Resource not found.",
                            details = exception.Message,
                            timestamp = DateTime.UtcNow,
                            requestId = context.TraceIdentifier
                        }
                    };
                    break;

                case InvalidOperationException:
                    context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    response = new
                    {
                        error = new
                        {
                            message = "Operation cannot be completed.",
                            details = exception.Message,
                            timestamp = DateTime.UtcNow,
                            requestId = context.TraceIdentifier
                        }
                    };
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }

        private string GetExceptionDetails(Exception exception)
        {
            // In development, show full exception details
            // In production, show generic message only
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            
            return environment == "Development" 
                ? $"{exception.Message} | {exception.StackTrace}"
                : "Please contact support if the problem persists.";
        }
    }
}