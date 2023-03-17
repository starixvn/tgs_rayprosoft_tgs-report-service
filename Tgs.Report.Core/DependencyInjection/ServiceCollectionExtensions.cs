// ReSharper disable CheckNamespace

using System.Reflection;
using MediatR;
using Tgs.Report.Core;
using Tgs.Report.Core.Queries.Market;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Market;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReportCore(this IServiceCollection services)
    {
        services.AddMediatR(r => r.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddTransient<IRoundFactory, RoundFactory>();
        services.AddTransient<IPlayerFactory, PlayerFactory>();
        services.AddTransient<IPlayerDepositFactory, PlayerDepositFactory>();

        services
            .AddTransient<IRequestHandler<GetMarketsRequest, PagedResponse<MarketModel>>, GetMarketsQueryHandler>()
            .Decorate<IRequestHandler<GetMarketsRequest, PagedResponse<MarketModel>>, GetMarketsCacheHandler>();
        return services;
    }
}