using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tgs.Report.Repository.Models;

namespace Tgs.Report.Repository.EntityTypeConfigurations
{
    internal class RoundDetailMap : IEntityTypeConfiguration<RoundDetail>
    {
        void IEntityTypeConfiguration<RoundDetail>.Configure(EntityTypeBuilder<RoundDetail> builder)
        {
            builder.ToTable("round_details", Consts.BetSchema);

            builder.HasKey(c => c.RoundId)
                .HasName("pk_round_details");

            builder.Property(c => c.AdditionalData)
                .IsRequired();

            builder.HasOne(c => c.Round)
                .WithOne(c => c.RoundDetail)
                .HasForeignKey<RoundDetail>(c => c.RoundId)
                .HasPrincipalKey<Round>(c => c.Id)
                .HasConstraintName("fk_round_details_rounds_round_id");
        }
    }
}