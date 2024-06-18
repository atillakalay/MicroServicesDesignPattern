using Polly;
using Polly.Extensions.Http;
using System.Diagnostics;
using System.Net;

namespace ServiceA.API
{
    public static class PolicyRegistry
    {
        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(3, TimeSpan.FromSeconds(10), onBreak: (arg1, arg2) =>
                {
                    Debug.WriteLine("Circuit Breaker Status => On Break");
                }, onReset: () =>
                {
                    Debug.WriteLine("Circuit Breaker Status => On Reset");
                }, onHalfOpen: () =>
                {
                    Debug.WriteLine("Circuit Breaker Status => On Half Open");
                });
        }

        public static IAsyncPolicy<HttpResponseMessage> GetAdvanceCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .AdvancedCircuitBreakerAsync(0.5, TimeSpan.FromSeconds(30), 30, TimeSpan.FromSeconds(30), onBreak: (arg1, arg2) =>
                {
                    Debug.WriteLine("Circuit Breaker Status => On Break");
                }, onReset: () =>
                {
                    Debug.WriteLine("Circuit Breaker Status => On Reset");
                }, onHalfOpen: () =>
                {
                    Debug.WriteLine("Circuit Breaker Status => On Half Open");
                });
        }

        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
                .WaitAndRetryAsync(5, retryAttempt =>
                {
                    Debug.WriteLine($"Retry Count :{retryAttempt}");
                    return TimeSpan.FromSeconds(10);
                }, onRetryAsync: OnRetryAsync);
        }

        private static Task OnRetryAsync(DelegateResult<HttpResponseMessage> arg1, TimeSpan arg2)
        {
            Debug.WriteLine($"Request is made again:{arg2.TotalMilliseconds}");
            return Task.CompletedTask;
        }
    }
}