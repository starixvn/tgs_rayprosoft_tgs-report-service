// ReSharper disable CheckNamespace

using System.Reflection;
using MediatR;
using TGS.Core.Infrastructures.Extensions.Models;
using Tgs.Report.Core;
using Tgs.Report.Core.Queries.Market;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Market;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReportCore(this IServiceCollection services)
    {
        services.AddMediatR(r => r.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()))
            .AddTransient<IRoundFactory, RoundFactory>()
            .AddTransient<IPlayerFactory, PlayerFactory>()
            .AddTransient<IPlayerDepositFactory, PlayerDepositFactory>()
            .AddTransient<IMarketFactory, MarketFactory>()
            .AddTransient<IAgentFactory, AgentFactory>();
        services
            .AddTransient<IRequestHandler<GetMarketsRequest, PagedResponse<MarketModel>>, GetMarketsQueryHandler>()
            .Decorate<IRequestHandler<GetMarketsRequest, PagedResponse<MarketModel>>, GetMarketsCacheHandler>();
        return services;
    }
}