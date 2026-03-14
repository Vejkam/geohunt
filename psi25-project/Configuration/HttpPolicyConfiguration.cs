using Polly;
using Polly.Extensions.Http;
using Serilog;

namespace psi25_project.Configuration
{
    public static class HttpPolicyConfiguration
    {
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            var random = new Random();

            var rateLimitPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => r.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt =>
                    {
                        var baseDelay = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt + 2));
                        var jitter = TimeSpan.FromMilliseconds(random.Next(0, 1000));
                        return baseDelay + jitter;
                    },
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        Log.Warning("Google Maps API rate limit hit (429). Retry {RetryCount} after {Delay}s.",
                            retryCount, timespan.TotalSeconds);
                    });

            var transientErrorPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt =>
                    {
                        var baseDelay = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                        var jitter = TimeSpan.FromMilliseconds(random.Next(0, 1000));
                        return baseDelay + jitter;
                    },
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        Log.Warning("Google Maps API call failed. Retry {RetryCount} after {Delay}s. Status: {StatusCode}",
                            retryCount, timespan.TotalSeconds, outcome.Result?.StatusCode);
                    });

            return Policy.WrapAsync(rateLimitPolicy, transientErrorPolicy);
        }

        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (outcome, duration) =>
                    {
                        Log.Error("Google Maps API circuit breaker opened for {Duration}s due to consecutive failures", duration.TotalSeconds);
                    },
                    onReset: () =>
                    {
                        Log.Information("Google Maps API circuit breaker reset - resuming normal operations");
                    });
        }
    }
}
