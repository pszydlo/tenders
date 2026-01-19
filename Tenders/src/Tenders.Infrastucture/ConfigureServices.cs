using System.Threading.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;
using Tenders.Application.Abstractions;
using Tenders.Infrastucture.Background;
using Tenders.Infrastucture.Caching;
using Tenders.Infrastucture.Options;
using Tenders.Infrastucture.Persistence;
using Tenders.Infrastucture.Repositories;
using Tenders.Infrastucture.TendersApi;

namespace Tenders.Infrastucture;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<TendersOptions>(configuration.GetSection(TendersOptions.SectionName));

        services.AddHttpClient<ITendersApiClient, TendersApiClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<TendersOptions>>().Value;
 
            client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
            client.Timeout = Timeout.InfiniteTimeSpan;
        })
        .AddStandardResilienceHandler(options =>
        {
            var limiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
            {
                TokenLimit = 20,
                TokensPerPeriod = 10,
                ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 200,
                AutoReplenishment = true
            });

            options.RateLimiter = new HttpRateLimiterStrategyOptions
            {
                RateLimiter = args => limiter.AcquireAsync(
                    permitCount: 1,
                    cancellationToken: args.Context.CancellationToken)
            };

            // --- TIMEOUTY ---
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(60);
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(250);

            // --- RETRY ---
            options.Retry.MaxRetryAttempts = 2;
            options.Retry.Delay = TimeSpan.FromSeconds(2);
            options.Retry.BackoffType = DelayBackoffType.Exponential;
            options.Retry.UseJitter = true;

            // --- BREAKER ---
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(120);
            options.CircuitBreaker.FailureRatio = 0.5;
            options.CircuitBreaker.MinimumThroughput = 30;
            options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(45);
        });

        services.AddSingleton<ITendersRepository, TendersRepository>();
        services.AddSingleton<ITendersPagesStore, FileTendersPagesStore>();
        services.AddSingleton<ITendersIndexProvider, TendersIndexProvider>();

        services.AddHostedService<TendersIndexBootstrapHostedService>();
        services.AddHostedService<TendersIndexRefreshHostedService>();
        return services;
    }
}
