using Hr360.Application.Interfaces;
using Hr360.Domain;
using Hr360.Infrastructure.Repositories;
using Hr360.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace Hr360.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHr360Infrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("Hr360");

        if (string.IsNullOrWhiteSpace(connectionString) && !environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "ConnectionStrings:Hr360 must be configured outside the Development environment. "
                + "The in-memory database fallback is only available in Development.");
        }

        services.AddDbContext<Hr360DbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            });
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IReviewTemplateRepository, ReviewTemplateRepository>();
        services.AddScoped<IReviewCycleRepository, ReviewCycleRepository>();
        services.AddScoped<IReviewAssignmentRepository, ReviewAssignmentRepository>();
        services.AddScoped<IFeedbackSubmissionRepository, FeedbackSubmissionRepository>();
        services.AddScoped<IAuditEventRepository, AuditEventRepository>();

        services.AddResiliencePipeline("outbound-api", builder =>
        {
            builder
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromMilliseconds(250),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true
                })
                .AddCircuitBreaker(new CircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.5,
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    MinimumThroughput = 10,
                    BreakDuration = TimeSpan.FromSeconds(30)
                })
                .AddTimeout(TimeSpan.FromSeconds(10));
        });

        return services;
    }
}
