using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tgs.Report.Repository.Models;

namespace Tgs.Report.Repository.EntityTypeConfigurations
{
    internal class BrandCurrencyMap : IEntityTypeConfiguration<BrandCurrency>
    {
        void IEntityTypeConfiguration<BrandCurrency>.Configure(EntityTypeBuilder<BrandCurrency> builder)
        {
            builder.ToTable("brand_currencies");

            builder.HasKey(e => new { e.BrandId, e.CurrencyCode });

            builder.Property(e => e.CurrencyCode)
                .HasMaxLength(16)
                .IsRequired();

            builder.Property(e => e.BrandCurrencyCode)
                .HasMaxLength(32)
                .IsRequired();

            builder.HasOne(e => e.Brand)
                .WithMany(e => e.Currencies)
                .HasForeignKey(e => e.BrandId)
                .HasConstraintName("fk_brand_currencies_brands_brand_id");

            builder.HasIndex(e => e.CurrencyCode)
                .HasDatabaseName("ix_brand_currencies_currency_code");
        }
    }
}