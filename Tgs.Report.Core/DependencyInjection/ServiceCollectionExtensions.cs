// ReSharper disable CheckNamespace

using System.Reflection;
using MediatR;
using Tgs.Report.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReportCore(this IServiceCollection services)
    {
        services.AddMediatR(Assembly.GetExecutingAssembly());
        services.AddTransient<IRoundFactory, RoundFactory>();
        services.AddTransient<IPlayerFactory, PlayerFactory>();
        services.AddTransient<IPlayerDepositFactory, PlayerDepositFactory>();

        return services;
    }
}