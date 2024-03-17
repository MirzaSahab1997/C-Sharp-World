using LMSOnboardingApiV3.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityVerificationService.Infrastructure.Configuration
{
    public class IDVerificationKybConfiguration : IEntityTypeConfiguration<IDVerificationKyb>
    {
        public void Configure(EntityTypeBuilder<IDVerificationKyb> builder)
        {
            builder.ToTable("tbl_kyb", "id_verification");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TransactionId)
                .HasColumnName("transaction_id");

            builder.Property(x => x.Data)
                .HasColumnName("data");

            builder.Property(x => x.ApplicationId)
                .HasColumnName("application_id");

            builder.Property(x => x.Status)
               .HasColumnName("status");

            builder.Property(x => x.CreatedDate)
                .HasColumnName("created_date");

            builder.Property(x => x.CreatedBy)
                .HasColumnName("created_by");

            builder.Property(x => x.ModifiedDate)
                .HasColumnName("modified_date");

            builder.Property(x => x.ModifiedBy)
                .HasColumnName("modified_by");
        }
    }
}