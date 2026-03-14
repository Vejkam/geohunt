using Microsoft.Extensions.Caching.Memory;
using Moq;
using psi25_project.Gateways.Interfaces;
using psi25_project.Models.Dtos;
using psi25_project.Services;

namespace Geohunt.Tests.Services
{
    public class GeocodingServiceTests : IDisposable
    {
        private sealed class AddressFileScope : IDisposable
        {
            private readonly string _path;
            private readonly bool _existed;
            private readonly string? _originalContents;

            public AddressFileScope(string singleAddressLine)
            {
                _path = Path.Combine(Directory.GetCurrentDirectory(), "addresses.txt");
                _existed = File.Exists(_path);
                _originalContents = _existed ? File.ReadAllText(_path) : null;

                File.WriteAllText(_path, singleAddressLine + Environment.NewLine);
            }

            public void Dispose()
            {
                if (_existed)
                {
                    File.WriteAllText(_path, _originalContents ?? string.Empty);
                }
                else
                {
                    if (File.Exists(_path))
                        File.Delete(_path);
                }
            }
        }

        private readonly Mock<IGoogleMapsGateway> _mockGateway;
        private readonly IMemoryCache _cache;
        private readonly GeocodingService _service;

        public GeocodingServiceTests()
        {
            _mockGateway = new Mock<IGoogleMapsGateway>();
            _cache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 100 });
            _service = new GeocodingService(_mockGateway.Object, _cache);
        }

        public void Dispose()
        {
            _cache.Dispose();
        }

        [Fact]
        public async Task GetValidCoordinatesAsync_CacheMiss_CallsGatewayAndCachesByNormalizedKey()
        {
            // Arrange
            var address = $"MiXeD CaSe {Guid.NewGuid():N}";
            using var _ = new AddressFileScope(address);

            var coords = new GeocodeResultDto { Lat = 40.7128, Lng = -74.0060 };
            _mockGateway.Setup(g => g.GetCoordinatesAsync(address)).ReturnsAsync(coords);
            _mockGateway.Setup(g => g.GetStreetViewMetadataAsync(It.IsAny<double>(), It.IsAny<double>()))
                .ReturnsAsync(new StreetViewLocationDto { PanoId = "test123" });

            // Act
            var (success, result) = await _service.GetValidCoordinatesAsync();

            // Assert
            Assert.True(success);
            Assert.NotNull(result);
            _mockGateway.Verify(g => g.GetCoordinatesAsync(address), Times.Once);

            var normalizedKey = address.ToLowerInvariant();
            Assert.True(_cache.TryGetValue(normalizedKey, out GeocodeResultDto? cached));
            Assert.NotNull(cached);
            Assert.Equal(coords.Lat, cached!.Lat);
            Assert.Equal(coords.Lng, cached.Lng);
        }

        [Fact]
        public async Task GetValidCoordinatesAsync_CacheHit_DoesNotCallGeocodeTwice()
        {
            // Arrange
            var address = $"Only Address {Guid.NewGuid():N}";
            using var _ = new AddressFileScope(address);

            _mockGateway.Setup(g => g.GetCoordinatesAsync(address))
                .ReturnsAsync(new GeocodeResultDto { Lat = 1, Lng = 2 });
            _mockGateway.Setup(g => g.GetStreetViewMetadataAsync(It.IsAny<double>(), It.IsAny<double>()))
                .ReturnsAsync(new StreetViewLocationDto { PanoId = "test123" });

            // Act
            var first = await _service.GetValidCoordinatesAsync();
            var second = await _service.GetValidCoordinatesAsync();

            // Assert
            Assert.True(first.success);
            Assert.True(second.success);
            _mockGateway.Verify(g => g.GetCoordinatesAsync(address), Times.Once);
            _mockGateway.Verify(g => g.GetStreetViewMetadataAsync(It.IsAny<double>(), It.IsAny<double>()), Times.Exactly(2));
        }

        [Fact]
        public async Task GetValidCoordinatesAsync_RetriesUntilStreetViewFound_ReturnsAttemptCount()
        {
            // Arrange
            var address = $"Retry Address {Guid.NewGuid():N}";
            using var _ = new AddressFileScope(address);

            _mockGateway.Setup(g => g.GetCoordinatesAsync(address))
                .ReturnsAsync(new GeocodeResultDto { Lat = 10, Lng = 20 });

            _mockGateway.SetupSequence(g => g.GetStreetViewMetadataAsync(It.IsAny<double>(), It.IsAny<double>()))
                .ReturnsAsync((StreetViewLocationDto?)null)
                .ReturnsAsync((StreetViewLocationDto?)null)
                .ReturnsAsync(new StreetViewLocationDto { PanoId = "pano-3" });

            // Act
            var (success, result) = await _service.GetValidCoordinatesAsync();

            // Assert
            Assert.True(success);
            Assert.NotNull(result);
            _mockGateway.Verify(g => g.GetStreetViewMetadataAsync(It.IsAny<double>(), It.IsAny<double>()), Times.Exactly(3));

            var attemptsProp = result.GetType().GetProperty("attempts");
            Assert.NotNull(attemptsProp);
            var attempts = (int)attemptsProp!.GetValue(result)!;
            Assert.Equal(3, attempts);
        }

        [Fact]
        public async Task GetValidCoordinatesAsync_ConcurrentCallsSameAddress_DeduplicatesGeocodeRequests()
        {
            // Arrange
            var address = $"Concurrent Address {Guid.NewGuid():N}";
            using var _ = new AddressFileScope(address);

            var callCount = 0;
            var tcs = new TaskCompletionSource<GeocodeResultDto>(TaskCreationOptions.RunContinuationsAsynchronously);

            _mockGateway.Setup(g => g.GetCoordinatesAsync(address))
                .Returns(() =>
                {
                    Interlocked.Increment(ref callCount);
                    return tcs.Task;
                });

            _mockGateway.Setup(g => g.GetStreetViewMetadataAsync(It.IsAny<double>(), It.IsAny<double>()))
                .ReturnsAsync(new StreetViewLocationDto { PanoId = "test123" });

            var tasks = Enumerable.Range(0, 5).Select(_i => _service.GetValidCoordinatesAsync()).ToArray();

            await Task.Yield();
            tcs.TrySetResult(new GeocodeResultDto { Lat = 1, Lng = 2 });

            await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(1, callCount);
            Assert.All(tasks, t => Assert.True(t.Result.success));
        }

        [Fact]
        public async Task GetValidCoordinatesAsync_WhenGeocodingThrows_AllowsRetryOnNextCall()
        {
            // Arrange
            var address = $"Throw Address {Guid.NewGuid():N}";
            using var _ = new AddressFileScope(address);

            _mockGateway.SetupSequence(g => g.GetCoordinatesAsync(address))
                .ThrowsAsync(new InvalidOperationException("boom"))
                .ReturnsAsync(new GeocodeResultDto { Lat = 1, Lng = 2 });

            _mockGateway.Setup(g => g.GetStreetViewMetadataAsync(It.IsAny<double>(), It.IsAny<double>()))
                .ReturnsAsync(new StreetViewLocationDto { PanoId = "test123" });

            // Act
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetValidCoordinatesAsync());
            var (success, _) = await _service.GetValidCoordinatesAsync();

            // Assert
            Assert.True(success);
            _mockGateway.Verify(g => g.GetCoordinatesAsync(address), Times.Exactly(2));
        }
    }
}
