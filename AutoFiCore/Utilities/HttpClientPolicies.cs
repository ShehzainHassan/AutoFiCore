using Polly;
using Polly.Extensions.Http;
using System.Net.Http;

public static class HttpClientPolicies
{
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int maxRetryAttempts = 3)
    {
        var random = new Random();

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                maxRetryAttempts,
                retryAttempt =>
                {
                    var baseDelay = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                    var jitter = TimeSpan.FromMilliseconds(random.Next(0, 1000));
                    return baseDelay + jitter;
                }
            );
    }

    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int failureThreshold = 5)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: failureThreshold,
                durationOfBreak: TimeSpan.FromSeconds(30)
            );
    }
}
