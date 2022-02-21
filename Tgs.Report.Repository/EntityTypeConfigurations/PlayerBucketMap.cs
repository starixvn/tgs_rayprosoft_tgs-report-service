using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tgs.Report.Repository.Models;

namespace Tgs.Report.Repository.EntityTypeConfigurations
{
    internal class PlayerBucketMap : IEntityTypeConfiguration<PlayerBucket>
    {
        void IEntityTypeConfiguration<PlayerBucket>.Configure(EntityTypeBuilder<PlayerBucket> builder)
        {
            builder.ToTable("player_buckets");

            builder.HasKey(c => c.Id)
                .HasName("pk_player_buckets");

            builder.Property(c => c.PlayerCode)
                .IsRequired();

            builder.Property(c => c.BucketAmount)
                .HasPrecision(18, 6)
                .IsRequired();

            builder.Property(c => c.CurrentBucketAmount)
                .HasPrecision(18, 6)
                .IsRequired();

            builder.Property(c => c.BucketType)
                .HasColumnType(ColumnTypes.SmallInt)
                .IsRequired();

            builder.Property(c => c.CreatedOn)
                .IsRequired();

            builder.HasIndex(c => c.PlayerCode)
                .HasDatabaseName("ix_player_buckets_player_code");
        }
    }
}