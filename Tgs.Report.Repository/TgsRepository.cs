using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Tgs.Report.Repository.Entities;

namespace Tgs.Report.Repository
{
    public class TgsRepository : DbContext, ITgsRepository
    {
        public TgsRepository(DbContextOptions<TgsRepository> options) : base(options)
        {
        }

        public DbSet<BrandGameProviderAggregate> BrandGameProviderAggregates { get; set; } = default!;
        public DbSet<BrandGameProviderAggregateAuditLog> BrandGameProviderAggregateAuditLogs { get; set; } = default!;
        public DbSet<BrandGameProviderAggregateRecord> BrandGameProviderAggregateRecords { get; set; } = default!;
        public DbSet<GameProviderAggregate> GameProviderAggregates { get; set; } = default!;
        public DbSet<GameProviderAggregateAuditLog> GameProviderAggregateAuditLogs { get; set; } = default!;
        public DbSet<GameProviderAggregateRecord> GameProviderAggregateRecords { get; set; } = default!;

        public DbSet<AgentDividendCondition> AgentDividendConditions { get; set; } = default!;

        public IDbContextTransaction? CurrentTransaction =>
            Database.CurrentTransaction;

        public DbSet<AgentDividendMultipleLayer> AgentDividendMultipleLayers { get; set; } = default!;
        public DbSet<AgentDividend> AgentDividends { get; set; } = default!;
        public DbSet<AgentTransaction> AgentTransactions { get; set; } = default!;
        public DbSet<Agent> Agents { get; set; } = default!;
        public DbSet<BrandCurrency> BrandCurrencies { get; set; } = default!;
        public DbSet<BrandGame> BrandGames { get; set; } = default!;
        public DbSet<Brand> Brands { get; set; } = default!;
        public DbSet<Currency> Currencies { get; set; } = default!;
        public DbSet<Department> Departments { get; set; } = default!;
        public DbSet<GameAction> GameActions { get; set; } = default!;
        public DbSet<GameProvider> GameProviders { get; set; } = default!;
        public DbSet<Game> Games { get; set; } = default!;
        public DbSet<Licensee> Licensees { get; set; } = default!;
        public DbSet<PlayerAggregate> PlayerAggregates { get; set; } = default!;
        public DbSet<PlayerBucketHistory> PlayerBucketHistories { get; set; } = default!;
        public DbSet<PlayerBucket> PlayerBuckets { get; set; } = default!;
        public DbSet<PlayerGameRebateAggregate> PlayerGameRebateAggregates { get; set; } = default!;
        public DbSet<Player> Players { get; set; } = default!;
        public DbSet<RoundAggregate> RoundAggregates { get; set; } = default!;
        public DbSet<WithdrawForm> WithdrawForms { get; set; } = default!;
        public DbSet<User> Users { get; set; } = default!;
        public DbSet<Transaction> Transactions { get; set; } = default!;
        public DbSet<Round> Rounds { get; set; } = default!;
        public DbSet<Setting> Settings { get; set; } = default!;
        public DbSet<RoundDetail> RoundDetails { get; set; } = default!;
        public DbSet<DepositForm> DepositForms { get; set; } = null!;
        public DbSet<PlayerIpRecord> PlayerIpRecords { get; set; } = null!;

        public IDbConnection GetDbConnection() =>
            Database.GetDbConnection();

        public Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel level) =>
            Database.BeginTransactionAsync(level);

        public Task<bool> CanConnectionAsync() => Database.CanConnectAsync();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("dbo");
            modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

            modelBuilder.HasPostgresExtension("pg_trgm");
        }
    }
}