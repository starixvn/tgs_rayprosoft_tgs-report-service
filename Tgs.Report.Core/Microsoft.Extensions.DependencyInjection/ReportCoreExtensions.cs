using System.Reflection;
using MediatR;
using Tgs.Report.Core;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ReportCoreExtensions
    {
        public static IServiceCollection AddReportCore(this IServiceCollection services)
        {
            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddTransient(typeof(ITgsService<>), typeof(TgsService<>));
            services.AddTransient<IPlayerAggregateFactory, PlayerAggregateFactory>();
            services.AddTransient<IPlayerBucketFactory, PlayerBucketFactory>();
            services.AddTransient<IPlayerGameAggregateFactory, PlayerGameAggregateFactory>();
            services.AddTransient<IRoundAggregateFactory, RoundAggregateFactory>();
            services.AddTransient<IRoundFactory, RoundFactory>();
            services.AddTransient<IPlayerFactory, PlayerFactory>();

            return services;
        }
    }
}