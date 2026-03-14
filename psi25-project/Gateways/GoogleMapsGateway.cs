using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using psi25_project.Exceptions;
using psi25_project.Gateways.Interfaces;
using psi25_project.Models.Dtos;

namespace psi25_project.Gateways
{
    public class GoogleMapsGateway : IGoogleMapsGateway
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        private readonly ILogger<GoogleMapsGateway> _logger;
        private readonly IMemoryCache _cache;
        private static readonly ConcurrentDictionary<string, Lazy<Task<StreetViewLocationDto?>>> _inFlightStreetViewRequests = new();

        public GoogleMapsGateway(HttpClient httpClient, IConfiguration configuration, ILogger<GoogleMapsGateway> logger, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _logger = logger;
            _cache = cache;
            _apiKey = configuration["GoogleMaps:ApiKey"]
                      ?? throw new GoogleMapsApiException(
                          endpoint: "Configuration",
                          errorCode: "MISSING_API_KEY",
                          message: "Google Maps API key not found in configuration. Please set GoogleMaps:ApiKey.");
        }

        private static string NormalizeCoordKey(double lat, double lng) => $"{lat:F6},{lng:F6}";

        public async Task<GeocodeResultDto> GetCoordinatesAsync(string address)
        {
            string endpoint = "Geocoding API";
            string url = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={_apiKey}";

            _logger.LogInformation("Calling Google Maps Geocoding API for address: {Address}", address);

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                string content = await response.Content.ReadAsStringAsync();

                // Check HTTP status
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Google Maps API returned HTTP {StatusCode} for geocoding request. Response: {Response}",
                        response.StatusCode, content);

                    throw new GoogleMapsApiException(
                        endpoint: endpoint,
                        statusCode: response.StatusCode,
                        errorCode: $"HTTP_{(int)response.StatusCode}",
                        message: $"Google Maps Geocoding API request failed with status {response.StatusCode}",
                        innerException: null);
                }

                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                // Check API status in response
                string? status = root.GetProperty("status").GetString();

                if (status != "OK")
                {
                    string? errorMessage = root.TryGetProperty("error_message", out var errMsg)
                        ? errMsg.GetString()
                        : $"Geocoding failed with status: {status}";

                    _logger.LogError("Google Maps Geocoding API returned error status: {Status}, Message: {ErrorMessage}",
                        status, errorMessage);

                    throw new GoogleMapsApiException(
                        endpoint: endpoint,
                        statusCode: response.StatusCode,
                        errorCode: status ?? "UNKNOWN_ERROR",
                        message: errorMessage ?? "Unknown geocoding error");
                }

                var location = root
                    .GetProperty("results")[0]
                    .GetProperty("geometry")
                    .GetProperty("location");

                var result = new GeocodeResultDto
                {
                    Lat = location.GetProperty("lat").GetDouble(),
                    Lng = location.GetProperty("lng").GetDouble()
                };

                _logger.LogInformation("Successfully geocoded address: {Address} to coordinates ({Lat}, {Lng})",
                    address, result.Lat, result.Lng);

                return result;
            }
            catch (GoogleMapsApiException)
            {
                throw; // Re-throw our custom exceptions
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error occurred while calling Google Maps Geocoding API for address: {Address}", address);

                throw new GoogleMapsApiException(
                    endpoint: endpoint,
                    statusCode: null,
                    errorCode: "NETWORK_ERROR",
                    message: "Network error occurred while contacting Google Maps API",
                    innerException: ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while processing geocoding response for address: {Address}", address);

                throw new GoogleMapsApiException(
                    endpoint: endpoint,
                    statusCode: null,
                    errorCode: "PROCESSING_ERROR",
                    message: "Failed to process Google Maps API response",
                    innerException: ex);
            }
        }

        public async Task<StreetViewLocationDto?> GetStreetViewMetadataAsync(double lat, double lng)
        {
            // Use normalized cache key for both cache and in-flight tracking
            var cacheKey = NormalizeCoordKey(lat, lng);

            StreetViewLocationDto? result;

            // Check cache first
            if (!_cache.TryGetValue(cacheKey, out result))
            {
                // Deduplicate in-flight requests using Lazy<Task<T>>
                var lazyTask = _inFlightStreetViewRequests.GetOrAdd(
                    cacheKey,
                    key => new Lazy<Task<StreetViewLocationDto?>>(
                        async () => await GetStreetViewMetadataAsyncInternal(lat, lng),
                        LazyThreadSafetyMode.ExecutionAndPublication
                    )
                );

                try
                {
                    result = await lazyTask.Value;

                    // Store in cache (even if null - that's a valid result)
                    _cache.Set(cacheKey, result, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
                        Size = 1
                    });
                }
                finally
                {
                    // Always remove from in-flight tracking
                    _inFlightStreetViewRequests.TryRemove(cacheKey, out _);
                }
            }

            return result;
        }

        private async Task<StreetViewLocationDto?> GetStreetViewMetadataAsyncInternal(double lat, double lng)
        {
            string endpoint = "Street View Metadata API";
            string url = $"https://maps.googleapis.com/maps/api/streetview/metadata?location={lat},{lng}&key={_apiKey}";

            _logger.LogInformation("Calling Google Street View Metadata API for coordinates: ({Lat}, {Lng})", lat, lng);

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                string content = await response.Content.ReadAsStringAsync();

                // Check HTTP status
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Google Maps API returned HTTP {StatusCode} for Street View metadata request. Response: {Response}",
                        response.StatusCode, content);

                    throw new GoogleMapsApiException(
                        endpoint: endpoint,
                        statusCode: response.StatusCode,
                        errorCode: $"HTTP_{(int)response.StatusCode}",
                        message: $"Google Street View Metadata API request failed with status {response.StatusCode}",
                        innerException: null);
                }

                var metadata = JsonSerializer.Deserialize<StreetViewMetadataDto>(content);

                if (metadata == null)
                {
                    _logger.LogError("Failed to deserialize Street View metadata response for coordinates: ({Lat}, {Lng})", lat, lng);

                    throw new GoogleMapsApiException(
                        endpoint: endpoint,
                        statusCode: response.StatusCode,
                        errorCode: "INVALID_RESPONSE",
                        message: "Failed to parse Street View metadata response");
                }

                // Check API status
                if (metadata.Status == "OK" && metadata.Location != null)
                {
                    var result = new StreetViewLocationDto
                    {
                        Lat = metadata.Location.Latitude,
                        Lng = metadata.Location.Longitude,
                        PanoId = metadata.PanoId
                    };

                    _logger.LogInformation("Street View available at ({Lat}, {Lng}) with PanoId: {PanoId}",
                        result.Lat, result.Lng, result.PanoId);

                    return result;
                }

                // Street View not available at this location (status: ZERO_RESULTS, etc.)
                _logger.LogWarning("Street View not available for coordinates: ({Lat}, {Lng}), Status: {Status}",
                    lat, lng, metadata.Status);

                return null;
            }
            catch (GoogleMapsApiException)
            {
                throw; // Re-throw our custom exceptions
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error occurred while calling Google Street View Metadata API for coordinates: ({Lat}, {Lng})",
                    lat, lng);

                throw new GoogleMapsApiException(
                    endpoint: endpoint,
                    statusCode: null,
                    errorCode: "NETWORK_ERROR",
                    message: "Network error occurred while contacting Google Maps API",
                    innerException: ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while processing Street View metadata for coordinates: ({Lat}, {Lng})",
                    lat, lng);

                throw new GoogleMapsApiException(
                    endpoint: endpoint,
                    statusCode: null,
                    errorCode: "PROCESSING_ERROR",
                    message: "Failed to process Google Street View metadata response",
                    innerException: ex);
            }
        }
    }
}
