using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using psi25_project.Exceptions;
using psi25_project.Middleware;
using System.Net;
using System.Text.Json;

namespace Geohunt.Tests.Middleware
{
    public class GlobalExceptionHandlerMiddlewareTests
    {
        private readonly Mock<ILogger<GlobalExceptionHandlerMiddleware>> _mockLogger;

        public GlobalExceptionHandlerMiddlewareTests()
        {
            _mockLogger = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
        }

        [Fact]
        public async Task InvokeAsync_GoogleMapsApiException_OverQueryLimit_ReturnsTooManyRequests()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            var middleware = new GlobalExceptionHandlerMiddleware(
                next: (ctx) => throw new GoogleMapsApiException("geocode", "OVER_QUERY_LIMIT", "Query limit exceeded"),
                logger: _mockLogger.Object
            );

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal(429, context.Response.StatusCode);
            Assert.Equal("application/json", context.Response.ContentType);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
            var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);

            Assert.Equal("MAPS_UNAVAILABLE", errorResponse.GetProperty("code").GetString());
            Assert.Equal("OVER_QUERY_LIMIT", errorResponse.GetProperty("details").GetProperty("errorCode").GetString());
        }

        [Fact]
        public async Task InvokeAsync_GoogleMapsApiException_RequestDenied_ReturnsBadRequest()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            var middleware = new GlobalExceptionHandlerMiddleware(
                next: (ctx) => throw new GoogleMapsApiException("geocode", "REQUEST_DENIED", "Request denied"),
                logger: _mockLogger.Object
            );

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal(400, context.Response.StatusCode);
            Assert.Equal("application/json", context.Response.ContentType);
        }

        [Fact]
        public async Task InvokeAsync_GoogleMapsApiException_MissingApiKey_ReturnsInternalServerError()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            var middleware = new GlobalExceptionHandlerMiddleware(
                next: (ctx) => throw new GoogleMapsApiException("geocode", "MISSING_API_KEY", "API key missing"),
                logger: _mockLogger.Object
            );

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal(500, context.Response.StatusCode);
            Assert.Equal("application/json", context.Response.ContentType);
        }

        [Fact]
        public async Task InvokeAsync_GoogleMapsApiException_NetworkError_ReturnsServiceUnavailable()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            var middleware = new GlobalExceptionHandlerMiddleware(
                next: (ctx) => throw new GoogleMapsApiException("geocode", "NETWORK_ERROR", "Network error"),
                logger: _mockLogger.Object
            );

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal(503, context.Response.StatusCode);
            Assert.Equal("application/json", context.Response.ContentType);
        }

        [Fact]
        public async Task InvokeAsync_GoogleMapsApiException_UnknownCode_UsesStatusCodeProperty()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            var middleware = new GlobalExceptionHandlerMiddleware(
                next: (ctx) => throw new GoogleMapsApiException(
                    "geocode",
                    HttpStatusCode.BadGateway,
                    "UNKNOWN_ERROR",
                    "Unknown error"),
                logger: _mockLogger.Object
            );

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal(502, context.Response.StatusCode);
            Assert.Equal("application/json", context.Response.ContentType);
        }

        [Fact]
        public async Task InvokeAsync_GoogleMapsApiException_ReturnsCorrectJsonStructure()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            var middleware = new GlobalExceptionHandlerMiddleware(
                next: (ctx) => throw new GoogleMapsApiException("geocode", "OVER_QUERY_LIMIT", "Query limit"),
                logger: _mockLogger.Object
            );

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
            var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);

            Assert.True(errorResponse.TryGetProperty("code", out _));
            Assert.True(errorResponse.TryGetProperty("message", out _));
            Assert.True(errorResponse.TryGetProperty("details", out var details));
            Assert.True(details.TryGetProperty("endpoint", out _));
            Assert.True(details.TryGetProperty("errorCode", out _));
            Assert.True(details.TryGetProperty("timestamp", out _));

            Assert.Equal("MAPS_UNAVAILABLE", errorResponse.GetProperty("code").GetString());
            Assert.Equal("geocode", details.GetProperty("endpoint").GetString());
            Assert.Equal("OVER_QUERY_LIMIT", details.GetProperty("errorCode").GetString());
        }

        [Fact]
        public async Task InvokeAsync_GoogleMapsApiException_LogsError()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            var exception = new GoogleMapsApiException("geocode", "OVER_QUERY_LIMIT", "Query limit");

            var middleware = new GlobalExceptionHandlerMiddleware(
                next: (ctx) => throw exception,
                logger: _mockLogger.Object
            );

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_GenericException_ReturnsInternalServerError()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            var middleware = new GlobalExceptionHandlerMiddleware(
                next: (ctx) => throw new InvalidOperationException("Something went wrong"),
                logger: _mockLogger.Object
            );

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal(500, context.Response.StatusCode);
            Assert.Equal("application/json", context.Response.ContentType);
        }

        [Fact]
        public async Task InvokeAsync_GenericException_ReturnsCorrectJsonStructure()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            var middleware = new GlobalExceptionHandlerMiddleware(
                next: (ctx) => throw new Exception("Generic error"),
                logger: _mockLogger.Object
            );

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
            var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);

            Assert.True(errorResponse.TryGetProperty("code", out _));
            Assert.True(errorResponse.TryGetProperty("message", out _));
            Assert.True(errorResponse.TryGetProperty("timestamp", out _));

            Assert.Equal("INTERNAL_ERROR", errorResponse.GetProperty("code").GetString());
            Assert.Equal("An internal server error occurred. Please try again later.", errorResponse.GetProperty("message").GetString());
        }

        [Fact]
        public async Task InvokeAsync_GenericException_LogsError()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            var exception = new Exception("Generic error");

            var middleware = new GlobalExceptionHandlerMiddleware(
                next: (ctx) => throw exception,
                logger: _mockLogger.Object
            );

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
