using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using psi25_project.Gateways;
using psi25_project.Exceptions;

namespace Geohunt.Tests.Gateways
{
    public class GoogleMapsGatewayTests
    {
        private GoogleMapsGateway CreateGateway(HttpMessageHandler handler, string apiKey = "fake-key")
        {
            var httpClient = new HttpClient(handler);
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c["GoogleMaps:ApiKey"]).Returns(apiKey);

            var logger = new Mock<ILogger<GoogleMapsGateway>>();
            var cache = new MemoryCache(new MemoryCacheOptions());

            return new GoogleMapsGateway(httpClient, configurationMock.Object, logger.Object, cache);
        }

        [Fact]
        public async Task GetCoordinatesAsync_ReturnsCoordinates_WhenApiReturnsOk()
        {
            var jsonResponse = @"{
                ""status"": ""OK"",
                ""results"": [{
                    ""geometry"": { ""location"": { ""lat"": 10.0, ""lng"": 20.0 } }
                }]
            }";

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            var gateway = CreateGateway(handlerMock.Object);
            var result = await gateway.GetCoordinatesAsync("Some Address");

            Assert.Equal(10.0, result.Lat);
            Assert.Equal(20.0, result.Lng);
        }

        [Fact]
        public async Task GetStreetViewMetadataAsync_ReturnsNull_WhenApiReturnsZeroResults()
        {
            var jsonResponse = @"{ ""status"": ""ZERO_RESULTS"" }";

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            var gateway = CreateGateway(handlerMock.Object);
            var result = await gateway.GetStreetViewMetadataAsync(10.0, 20.0);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetCoordinatesAsync_Throws_WhenApiReturnsErrorStatus()
        {
            var jsonResponse = @"{ ""status"": ""REQUEST_DENIED"", ""error_message"": ""Invalid API key"" }";

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            var gateway = CreateGateway(handlerMock.Object);

            var ex = await Assert.ThrowsAsync<GoogleMapsApiException>(() => gateway.GetCoordinatesAsync("Some Address"));
            Assert.Equal("REQUEST_DENIED", ex.ErrorCode);
        }
    }
}
