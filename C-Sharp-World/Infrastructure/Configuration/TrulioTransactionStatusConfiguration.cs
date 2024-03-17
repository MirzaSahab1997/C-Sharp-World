using IdentityVerificationService.Domain.Entities.IdVerification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityVerificationService.Infrastructure.Configuration
{
    public class TrulioTransactionStatusConfiguration : IEntityTypeConfiguration<IDVerificationTransactionStatus>
    {
        public void Configure(EntityTypeBuilder<IDVerificationTransactionStatus> builder)
        {
            builder.ToTable("tbl_transaction_status", "id_verification");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TransactionId)
                .HasColumnName("transaction_id");

            builder.Property(x => x.Status)
                .HasColumnName("status_data");

            builder.Property(x => x.CreatedDate)
                .HasColumnName("created_date");

            builder.Property(x => x.CreatedBy)
                .HasColumnName("created_by");
        }
    }
}