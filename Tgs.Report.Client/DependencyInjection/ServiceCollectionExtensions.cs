using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Tgs.Report.Client;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    [Obsolete("Please use ReportOptions implementation")]
    public static IServiceCollection AddReportClient(this IServiceCollection services,
        IConfiguration configuration, string configSectionName = "Report")
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), ServiceLifetime.Singleton);
        services.Configure<ClientSettings>(configuration.GetSection(configSectionName));

        services.AddSingleton(sp => sp.GetRequiredService<IOptions<ClientSettings>>().Value);

        services.AddHttpClient<IReportClient, ReportClient>((sp, c) =>
        {
            var settings = sp.GetRequiredService<ClientSettings>();
            c.BaseAddress = new(settings.Url);
        });

        return services;
    }

    public static IServiceCollection AddReportClient(this IServiceCollection services,
        Action<ReportOptions, IServiceProvider> configure)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), ServiceLifetime.Singleton);
        services.AddOptions<ReportOptions>().Configure(configure);

        services.AddHttpClient<IReportClient, ReportClient>((sp, c) =>
        {
            var options = sp.GetRequiredService<IOptions<ReportOptions>>().Value;
            c.BaseAddress = options.BaseUrl;
        });

        return services;
    }
}