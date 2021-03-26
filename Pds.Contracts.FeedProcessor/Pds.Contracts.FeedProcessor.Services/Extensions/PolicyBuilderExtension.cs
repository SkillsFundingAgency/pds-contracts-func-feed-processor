using Microsoft.Extensions.DependencyInjection;
using Pds.Contracts.FeedProcessor.Services.Configuration;
using Polly;
using Polly.Extensions.Http;
using Polly.Registry;
using System;

namespace Pds.Contracts.FeedProcessor.Services.Extensions
{
    /// <summary>
    /// Extension methods to add Polly policies.
    /// </summary>
    public static class PolicyBuilderExtension
    {
        /// <summary>
        /// Adds a set of rety and circuit breaker policies to the given policy registery with the
        /// given service name.
        /// </summary>
        /// <typeparam name="TService">The service for which the policies are being setup.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection" /> to add the feature's services to.</param>
        /// <param name="policyOptions">The policy options.</param>
        /// <param name="policyRegistry">The <see cref="IPolicyRegistry{TKey}" /> to add the new policies to.</param>
        /// <returns>
        /// A reference to this instance after the operation has completed.
        /// </returns>
        public static IServiceCollection AddPolicies<TService>(
            this IServiceCollection services,
            HttpPolicyOptions policyOptions,
            IPolicyRegistry<string> policyRegistry)
        {
            policyRegistry?.Add(
                $"{typeof(TService).Name}_{PolicyType.Retry}",
                HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .WaitAndRetryAsync(
                        policyOptions.HttpRetryCount,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(policyOptions.HttpRetryBackoffPower, retryAttempt))));

            policyRegistry?.Add(
                $"{typeof(TService).Name}_{PolicyType.CircuitBreaker}",
                HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .CircuitBreakerAsync(
                        handledEventsAllowedBeforeBreaking: policyOptions.CircuitBreakerToleranceCount,
                        durationOfBreak: policyOptions.CircuitBreakerDurationOfBreak));

            return services;
        }
    }
}