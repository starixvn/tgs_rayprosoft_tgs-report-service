using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tgs.Report.Repository.Configurations;

namespace Tgs.Report.Repository.Entities.Configurations
{
    internal class RoundMap : IEntityTypeConfiguration<Round>
    {
        void IEntityTypeConfiguration<Round>.Configure(EntityTypeBuilder<Round> builder)
        {
            builder.ToTable("rounds", "bet");

            builder.HasKey(bc => bc.Id)
                .HasName("pk_rounds");

            builder.Property(c => c.ExternalRoundId)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(c => c.GameProviderId)
                .IsRequired();

            builder.Property(c => c.PlayerCode)
                .IsRequired();

            builder.Property(c => c.Status)
                .HasColumnType(ColumnTypes.SmallInt)
                .IsRequired();

            builder.Property(c => c.GameId)
                .IsRequired();

            builder.Property(c => c.CurrencyCode)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(c => c.GameProviderConfigurationId)
                .IsRequired();

            builder.Property(c => c.WalletId)
                .IsRequired();

            builder.Property(c => c.BrandId)
                .IsRequired();

            builder.Property(c => c.CreatedOn)
                .IsRequired();

            builder.Property(c => c.ClosedOn);

            builder.Property(c => c.UpdatedOn)
                .IsRequired();

            builder.Property(c => c.ExternalCreatedOn);

            builder.Property(c => c.ExternalClosedOn);

            builder.Property(c => c.BetAmount)
                .HasPrecision(18, 6);

            builder.Property(c => c.ValidBetAmount)
                .HasPrecision(18, 6);

            builder.Property(c => c.WinLoss)
                .HasPrecision(18, 6);

            builder.Property(c => c.TieBetAdjustment)
                .HasPrecision(18, 6);

            builder.Property(c => c.Commission)
                .HasPrecision(18, 6);

            builder.Property(c => c.EffectiveBettingPercentage)
                .HasPrecision(18, 6);

            builder.Property(c => c.ClientStatus)
                .HasColumnType(ColumnTypes.SmallInt)
                .IsRequired();

            builder.HasIndex(c => c.GameProviderId)
                .HasDatabaseName("ix_rounds_game_provider_id");

            builder.HasIndex(c => c.PlayerCode)
                .HasDatabaseName("ix_rounds_player_code");

            builder.HasIndex(c => c.GameId)
                .HasDatabaseName("ix_rounds_game_id");

            builder.HasIndex(c => c.BrandId)
                .HasDatabaseName("ix_rounds_brand_id");

            builder.HasIndex(c => c.ClosedOn)
                .HasMethod("brin")
                .HasDatabaseName("ix_rounds_closed_on");

            builder.HasIndex(c => new { c.ExternalRoundId, c.PlayerCode, c.GameProviderId, c.GameId })
                .IsUnique()
                .IncludeProperties(c => new { c.GameProviderConfigurationId, c.WalletId })
                .HasDatabaseName("ux_rounds_external_round_id_composite");
        }
    }
}