using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Tgs.Report.Repository.Configurations;

namespace Tgs.Report.Repository
{
    public interface ITgsRepository
    {
        DbSet<AgentDividendCondition> AgentDividendConditions { get; init; }
        DbSet<AgentDividendMultipleLayer> AgentDividendMultipleLayers { get; init; }
        DbSet<AgentDividend> AgentDividends { get; init; }
        DbSet<AgentTransaction> AgentTransactions { get; init; }
        DbSet<Agent> Agents { get; init; }
        DbSet<BrandCurrency> BrandCurrencies { get; init; }
        DbSet<BrandGame> BrandGames { get; init; }
        DbSet<Brand> Brands { get; init; }
        DbSet<Currency> Currencies { get; init; }
        DbSet<Department> Departments { get; init; }
        DbSet<GameAction> GameActions { get; init; }
        DbSet<GameProvider> GameProviders { get; init; }
        DbSet<Game> Games { get; init; }
        DbSet<Licensee> Licensees { get; init; }
        DbSet<PlayerAggregate> PlayerAggregates { get; init; }
        DbSet<PlayerBucketHistory> PlayerBucketHistories { get; init; }
        DbSet<PlayerBucket> PlayerBuckets { get; init; }
        DbSet<PlayerGameRebateAggregate> PlayerGameRebateAggregates { get; init; }
        DbSet<Player> Players { get; init; }
        DbSet<RoundAggregate> RoundAggregates { get; init; }
        DbSet<WithdrawForm> WithdrawForms { get; init; }
        DbSet<User> Users { get; init; }
        DbSet<Transaction> Transactions { get; init; }
        DbSet<Round> Rounds { get; init; }
        DbSet<Setting> Settings { get; init; }
        DbSet<RoundDetail> RoundDetails { get; init; }

        IDbContextTransaction CurrentTransaction { get; }
        DbSet<T> Set<T>() where T : class;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        IDbConnection GetDbConnection();
    }
}