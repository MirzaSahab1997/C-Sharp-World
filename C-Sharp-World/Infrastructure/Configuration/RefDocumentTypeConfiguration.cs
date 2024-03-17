using IdentityVerificationService.Domain.Entities.IdVerification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityVerificationService.Infrastructure.Configuration
{
    public class RefDocumentTypeConfiguration : IEntityTypeConfiguration<RefDocumentType>
    {
        public void Configure(EntityTypeBuilder<RefDocumentType> builder)
        {
            builder.ToTable("tbl_ref_document_type", "id_verification");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .HasColumnName("name");

            builder.Property(x => x.IsActive)
                .HasColumnName("is_active");

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