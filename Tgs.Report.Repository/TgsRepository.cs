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

        public DbSet<AgentDividendCondition> AgentDividendConditions { get; init; }

        public IDbContextTransaction CurrentTransaction =>
            Database.CurrentTransaction;

        public DbSet<AgentDividendMultipleLayer> AgentDividendMultipleLayers { get; init; }
        public DbSet<AgentDividend> AgentDividends { get; init; }
        public DbSet<AgentTransaction> AgentTransactions { get; init; }
        public DbSet<Agent> Agents { get; init; }
        public DbSet<BrandCurrency> BrandCurrencies { get; init; }
        public DbSet<BrandGame> BrandGames { get; init; }
        public DbSet<Brand> Brands { get; init; }
        public DbSet<Currency> Currencies { get; init; }
        public DbSet<Department> Departments { get; init; }
        public DbSet<GameAction> GameActions { get; init; }
        public DbSet<GameProvider> GameProviders { get; init; }
        public DbSet<Game> Games { get; init; }
        public DbSet<Licensee> Licensees { get; init; }
        public DbSet<PlayerAggregate> PlayerAggregates { get; init; }
        public DbSet<PlayerBucketHistory> PlayerBucketHistories { get; init; }
        public DbSet<PlayerBucket> PlayerBuckets { get; init; }
        public DbSet<PlayerGameRebateAggregate> PlayerGameRebateAggregates { get; init; }
        public DbSet<Player> Players { get; init; }
        public DbSet<RoundAggregate> RoundAggregates { get; init; }
        public DbSet<WithdrawForm> WithdrawForms { get; init; }
        public DbSet<User> Users { get; init; }
        public DbSet<Transaction> Transactions { get; init; }
        public DbSet<Round> Rounds { get; init; }
        public DbSet<Setting> Settings { get; init; }
        public DbSet<RoundDetail> RoundDetails { get; init; }

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