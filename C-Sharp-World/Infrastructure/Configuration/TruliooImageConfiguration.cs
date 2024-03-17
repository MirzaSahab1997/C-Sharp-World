using IdentityVerificationService.Domain.Entities.IdVerification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityVerificationService.Infrastructure.Configuration
{
    public class TruliooImageConfiguration : IEntityTypeConfiguration<IDVerificationImage>
    {
        public void Configure(EntityTypeBuilder<IDVerificationImage> builder)
        {
            builder.ToTable("tbl_image", "id_verification");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.DocumentTypeId)
                .HasColumnName("document_type_id");

            builder.Property(x => x.DocumentFrontImage)
                .HasColumnName("front_image");

            builder.Property(x => x.DocumentBackImage)
                .HasColumnName("back_image");

            builder.Property(x => x.SelfieImage)
                .HasColumnName("selfie_image");

            builder.Property(x => x.UserId)
                .HasColumnName("application_id");

            builder.Property(x => x.IsDocumentVerified)
                .HasColumnName("is_veified");

            builder.Property(x => x.IsDocumentMatched)
                .HasColumnName("is_matched");

            builder.Property(x => x.ServiceStatusId)
                .HasColumnName("service_status_id");

            builder.Property(x => x.TransactionId)
                .HasColumnName("transaction_id");

            builder.Property(x => x.CreatedDate)
                .HasColumnName("created_date");

            builder.Property(x => x.CreatedBy)
                .HasColumnName("created_by");
        }
    }
}