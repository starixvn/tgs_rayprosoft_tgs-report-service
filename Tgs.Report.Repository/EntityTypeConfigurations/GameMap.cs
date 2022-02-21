using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tgs.Report.Repository.Models;

namespace Tgs.Report.Repository.EntityTypeConfigurations
{
    internal class GameMap : BaseConfiguration, IEntityTypeConfiguration<Game>
    {
        void IEntityTypeConfiguration<Game>.Configure(EntityTypeBuilder<Game> builder)
        {
            builder.ToTable("games");

            ConfigureBaseProperties<Game, int>(builder, "pk_games");

            builder.Property(e => e.Id)
                .ValueGeneratedNever();

            builder.Property(e => e.Code)
                .HasMaxLength(128)
                .IsRequired();

            builder.Property(e => e.Status)
                .HasColumnType(ColumnTypes.SmallInt)
                .IsRequired();

            builder.Property(c => c.ExternalId)
                .HasMaxLength(32)
                .IsRequired();

            builder.Property(e => e.Description)
                .HasMaxLength(128);

            builder.Property(e => e.EndpointPath)
                .HasMaxLength(2048);

            builder.Property(e => e.RuleUrl)
                .HasMaxLength(512);

            builder.Property(e => e.Type)
                .HasColumnType(ColumnTypes.SmallInt)
                .IsRequired();

            builder.Property(e => e.PlatformType)
                .HasColumnType(ColumnTypes.SmallInt)
                .IsRequired();

            builder.Property(e => e.SubGame)
                .IsRequired();

            builder.Property(e => e.AvailableToBrand)
                .IsRequired();

            builder.Property(e => e.HasTurnoverOnTieBet)
                .IsRequired();

            builder.Property(e => e.Html5)
                .IsRequired();

            builder.Property(e => e.AvailableCurrencies)
                .HasMaxLength(1024)
                .IsRequired()
                .HasDefaultValue(string.Empty);

            builder.Property(e => e.Localization)
                .HasColumnType(ColumnTypes.Jsonb);

            builder.Property(e => e.SubGameProviderCode)
                .HasMaxLength(16);

            builder.HasOne(e => e.GameProvider)
                .WithMany(e => e.Games)
                .HasForeignKey(e => e.GameProviderId)
                .HasConstraintName("fk_games_game_providers_game_provider_id");

            builder.HasIndex(e => e.GameProviderId)
                .HasDatabaseName("ix_games_game_provider_id");

            builder.HasIndex(e => e.SubGameProviderCode)
                .HasDatabaseName("ix_games_sub_game_provider_code");
        }
    }
}