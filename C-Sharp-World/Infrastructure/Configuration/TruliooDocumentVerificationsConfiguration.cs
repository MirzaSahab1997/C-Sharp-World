using IdentityVerificationService.Domain.Entities.IdVerification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityVerificationService.Infrastructure.Configuration
{
    public class TruliooDocumentVerificationsConfiguration : IEntityTypeConfiguration<IDVerificationDocument>
    {
        public void Configure(EntityTypeBuilder<IDVerificationDocument> builder)
        {
            builder.ToTable("tbl_document_verification", "id_verification");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TransactionId)
                .HasColumnName("transaction_id");

            builder.Property(x => x.Data)
                .HasColumnName("data");

            builder.Property(x => x.IsDocumentMatched)
                .HasColumnName("is_matched");

            builder.Property(x => x.UserId)
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