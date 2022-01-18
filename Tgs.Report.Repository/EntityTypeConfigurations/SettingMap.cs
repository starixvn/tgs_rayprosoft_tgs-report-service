using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tgs.Report.Repository.Models;

namespace Tgs.Report.Repository.EntityTypeConfigurations
{
    internal class SettingMap : IEntityTypeConfiguration<Setting>
    {
        void IEntityTypeConfiguration<Setting>.Configure(EntityTypeBuilder<Setting> builder)
        {
            builder.ToTable("settings");

            builder.HasKey(c => c.Key)
                .HasName("pk_settings");

            builder.Property(c => c.Key)
                .HasMaxLength(32)
                .IsRequired()
                .IsUnicode();

            builder.Property(c => c.Value)
                .IsRequired();

            builder.Property(c => c.Description)
                .HasMaxLength(256);

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
        }
    }
}