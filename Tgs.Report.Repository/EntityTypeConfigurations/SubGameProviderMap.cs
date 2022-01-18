using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tgs.Report.Repository.Models;

namespace Tgs.Report.Repository.EntityTypeConfigurations
{
    public class SubGameProviderMap : IEntityTypeConfiguration<SubGameProvider>
    {
        void IEntityTypeConfiguration<SubGameProvider>.Configure(EntityTypeBuilder<SubGameProvider> builder)
        {
            builder.ToTable("sub_game_providers");

            builder.HasKey(c => c.Code)
                .HasName("pk_sub_game_providers");

            builder.Property(c => c.Code)
                .HasMaxLength(16)
                .IsRequired();

            builder.Property(c => c.Name)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(e => e.Description)
                .HasMaxLength(128);

            builder.Property(e => e.GameProviderId)
                .IsRequired();

            builder.Property(e => e.Localization)
                .HasColumnType(ColumnTypes.Jsonb);

            builder.Property(c => c.CreatedByUsername)
                .HasMaxLength(32)
                .IsRequired();

            builder.Property(c => c.UpdatedByUsername)
                .HasMaxLength(32)
                .IsRequired();

            builder.Property(c => c.CreatedOn)
                .IsRequired();

            builder.Property(c => c.UpdatedOn)
                .IsRequired();

            builder.HasOne(c => c.GameProvider)
                .WithMany(c => c.SubGameProviders)
                .HasForeignKey(c => c.GameProviderId)
                .HasConstraintName("fk_sub_game_providers_game_providers_game_provider_id");

            builder.HasIndex(e => e.GameProviderId)
                .HasDatabaseName("ix_sub_game_providers_game_provider_id");
        }
    }
}