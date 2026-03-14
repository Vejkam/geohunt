using System.Net;
using System.Text.Json;
using psi25_project.Exceptions;

namespace psi25_project.Middleware
{
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
            catch (GoogleMapsApiException ex)
            {
                await HandleGoogleMapsApiExceptionAsync(context, ex);
            }
            catch (Exception ex)
            {
                await HandleGenericExceptionAsync(context, ex);
            }
        }

        private async Task HandleGoogleMapsApiExceptionAsync(HttpContext context, GoogleMapsApiException exception)
        {
            _logger.LogError(exception,
                "Google Maps API error occurred. Endpoint: {Endpoint}, ErrorCode: {ErrorCode}, StatusCode: {StatusCode}",
                exception.Endpoint,
                exception.ErrorCode,
                exception.StatusCode);

            if (context.Response.HasStarted)
            {
                _logger.LogWarning("Response already started; skipping error write. Path: {Path}", context.Request.Path);
                return;
            }

            context.Response.ContentType = "application/json";

            context.Response.StatusCode = exception.ErrorCode switch
            {
                "OVER_QUERY_LIMIT" or "OVER_DAILY_LIMIT" => (int)HttpStatusCode.TooManyRequests, // 429
                "REQUEST_DENIED" or "INVALID_REQUEST" => (int)HttpStatusCode.BadRequest, // 400
                "MISSING_API_KEY" => (int)HttpStatusCode.InternalServerError, // 500
                "NETWORK_ERROR" => (int)HttpStatusCode.ServiceUnavailable, // 503
                _ when exception.StatusCode.HasValue => (int)exception.StatusCode.Value,
                _ => (int)HttpStatusCode.ServiceUnavailable // 503
            };

            var errorResponse = new
            {
                code = "MAPS_UNAVAILABLE",
                message = "Map service temporarily unavailable. Please retry.",
                details = new
                {
                    endpoint = exception.Endpoint,
                    errorCode = exception.ErrorCode,
                    timestamp = DateTime.UtcNow
                }
            };

            var jsonResponse = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(jsonResponse);
        }

        private async Task HandleGenericExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred");

            if (context.Response.HasStarted)
            {
                _logger.LogWarning("Response already started; skipping error write. Path: {Path}", context.Request.Path);
                return;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var errorResponse = new
            {
                code = "INTERNAL_ERROR",
                message = "An internal server error occurred. Please try again later.",
                timestamp = DateTime.UtcNow
            };

            var jsonResponse = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
