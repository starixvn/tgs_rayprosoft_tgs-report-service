using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Tgs.Report.Repository.Entities;

namespace Tgs.Report.Repository
{
    public interface ITgsRepository
    {
        DbSet<AdjustPlayerRecord> AdjustPlayerRecords { get; set; }
        DbSet<AgentDividendCondition> AgentDividendConditions { get; set; }
        DbSet<AgentDividendMultipleLayer> AgentDividendMultipleLayers { get; set; }
        DbSet<AgentDividend> AgentDividends { get; set; }
        DbSet<AgentTransaction> AgentTransactions { get; set; }
        DbSet<Agent> Agents { get; set; }
        DbSet<BrandCurrency> BrandCurrencies { get; set; }
        DbSet<BrandGame> BrandGames { get; set; }
        DbSet<Brand> Brands { get; set; }
        DbSet<Currency> Currencies { get; set; }
        DbSet<Department> Departments { get; set; }
        DbSet<GameAction> GameActions { get; set; }
        DbSet<GameProvider> GameProviders { get; set; }
        DbSet<Game> Games { get; set; }
        DbSet<Licensee> Licensees { get; set; }
        DbSet<PlayerAggregate> PlayerAggregates { get; set; }
        DbSet<PlayerBucketHistory> PlayerBucketHistories { get; set; }
        DbSet<PlayerBucket> PlayerBuckets { get; set; }
        DbSet<PlayerGameRebateAggregate> PlayerGameRebateAggregates { get; set; }
        DbSet<Player> Players { get; set; }
        DbSet<RoundAggregate> RoundAggregates { get; set; }
        DbSet<WithdrawForm> WithdrawForms { get; set; }
        DbSet<User> Users { get; set; }
        DbSet<Transaction> Transactions { get; set; }
        DbSet<Round> Rounds { get; set; }
        DbSet<Setting> Settings { get; set; }
        DbSet<RoundDetail> RoundDetails { get; set; }
        DbSet<BrandGameProviderAggregate> BrandGameProviderAggregates { get; set; }
        DbSet<BrandGameProviderAggregateAuditLog> BrandGameProviderAggregateAuditLogs { get; set; }
        DbSet<BrandGameProviderAggregateRecord> BrandGameProviderAggregateRecords { get; set; }
        DbSet<GameProviderAggregate> GameProviderAggregates { get; set; }
        DbSet<GameProviderAggregateAuditLog> GameProviderAggregateAuditLogs { get; set; }
        DbSet<GameProviderAggregateRecord> GameProviderAggregateRecords { get; set; }
        DbSet<DepositForm> DepositForms { get; set; }
        DbSet<PlayerIpRecord> PlayerIpRecords { get; set; }

        IDbContextTransaction? CurrentTransaction { get; }

        DbSet<T> Set<T>() where T : class;

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        IDbConnection GetDbConnection();
    }
}