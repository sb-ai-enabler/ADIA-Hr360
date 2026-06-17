using Hr360.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Hr360.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHr360Application(this IServiceCollection services)
    {
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<IReviewCycleService, ReviewCycleService>();
        services.AddScoped<IFeedbackService, FeedbackService>();
        services.AddScoped<IReportingService, ReportingService>();
        return services;
    }
}
