using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Tgs.Report.Repository.Configurations;

namespace Tgs.Report.Repository
{
    public class TgsRepository : DbContext, ITgsRepository
    {
        public TgsRepository(DbContextOptions<TgsRepository> options) : base(options)
        {
        }

        public DbSet<BrandGameProviderAggregate> BrandGameProviderAggregates { get; set; }
        public DbSet<BrandGameProviderAggregateAuditLog> BrandGameProviderAggregateAuditLogs { get; set; }
        public DbSet<BrandGameProviderAggregateRecord> BrandGameProviderAggregateRecords { get; set; }
        public DbSet<GameProviderAggregate> GameProviderAggregates { get; set; }
        public DbSet<GameProviderAggregateAuditLog> GameProviderAggregateAuditLogs { get; set; }
        public DbSet<GameProviderAggregateRecord> GameProviderAggregateRecords { get; set; }

        public DbSet<AgentDividendCondition> AgentDividendConditions { get; set; }

        public IDbContextTransaction CurrentTransaction =>
            Database.CurrentTransaction;

        public DbSet<AgentDividendMultipleLayer> AgentDividendMultipleLayers { get; set; }
        public DbSet<AgentDividend> AgentDividends { get; set; }
        public DbSet<AgentTransaction> AgentTransactions { get; set; }
        public DbSet<Agent> Agents { get; set; }
        public DbSet<BrandCurrency> BrandCurrencies { get; set; }
        public DbSet<BrandGame> BrandGames { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<GameAction> GameActions { get; set; }
        public DbSet<GameProvider> GameProviders { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Licensee> Licensees { get; set; }
        public DbSet<PlayerAggregate> PlayerAggregates { get; set; }
        public DbSet<PlayerBucketHistory> PlayerBucketHistories { get; set; }
        public DbSet<PlayerBucket> PlayerBuckets { get; set; }
        public DbSet<PlayerGameRebateAggregate> PlayerGameRebateAggregates { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<RoundAggregate> RoundAggregates { get; set; }
        public DbSet<WithdrawForm> WithdrawForms { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Round> Rounds { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<RoundDetail> RoundDetails { get; set; }

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