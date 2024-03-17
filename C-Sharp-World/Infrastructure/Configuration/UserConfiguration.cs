using IdentityVerificationService.Domain.Entities.dbo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityVerificationService.Infrastructure.Configuration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("tbl_users", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.RoleId)
                .HasColumnName("RoleId");

            builder.Property(x => x.Name)
                .HasColumnName("name");

            builder.Property(x => x.Email)
                .HasColumnName("email");

            builder.Property(x => x.FingerPrintId)
                .HasColumnName("FingerPrint_Id");

            builder.Property(x => x.FaceId)
                .HasColumnName("Face_Id");

            builder.Property(x => x.EmailVerifiedAt)
                .HasColumnName("email_verified_at");

            builder.Property(x => x.Password)
                .HasColumnName("password");

            builder.Property(x => x.RememberToken)
                .HasColumnName("remember_token");

            builder.Property(x => x.CreatedDate)
                .HasColumnName("created_at");

            builder.Property(x => x.UpdatedDate)
                .HasColumnName("updated_at");

            builder.Property(x => x.IsApplicationSubmitted)
                .HasColumnName("ProfileCompleted");

            builder.Property(x => x.IsDeleted)
                .HasColumnName("IsDeleted");

            builder.Property(x => x.DeviceId)
                .HasColumnName("Device_Id");

            builder.Property(x => x.OS)
                .HasColumnName("OS");

            builder.Property(x => x.Model)
                .HasColumnName("Model");

            builder.Property(x => x.IsLocked)
                .HasColumnName("IsLock");

            builder.Property(x => x.LockedDate)
                .HasColumnName("LockedOn");

            builder.Property(x => x.ParentUserId)
                .HasColumnName("Parent_UserId");

            builder.Property(x => x.ParentStakeHolderId)
                .HasColumnName("Parent_StakholderId");

            builder.Property(x => x.IsStakeholder)
                .HasColumnName("IsStakeholder");

            builder.Property(x => x.ProfileImage)
                .HasColumnName("ProfileImage");

            builder.Property(x => x.SensibillId)
                .HasColumnName("Sensibill_ID");

            builder.Property(x => x.QbToken)
                .HasColumnName("QB_Token");

            builder.Property(x => x.QbCreatedDate)
                .HasColumnName("QB_CreatedDate");

            builder.Property(x => x.QbCompanyId)
                .HasColumnName("QB_CompanyId");

            builder.Property(x => x.QbRefreshToken)
                .HasColumnName("QB_RefresshToken");

            builder.Property(x => x.IsFirstLogin)
                .HasColumnName("FirstLogin");

            builder.Property(x => x.IsActive)
                .HasColumnName("IsActive");

            builder.Property(x => x.IsRejected)
                .HasColumnName("IsRejected");

            builder.Property(x => x.IsSigningOfficerAuthenticated)
                .HasColumnName("AuthenticatedSigningOfficer");

            builder.Property(x => x.CanDoElectronicCommunication)
                .HasColumnName("electronicCommunication");

            builder.Property(x => x.BankNumber)
                .HasColumnName("BankNumber");

            builder.Property(x => x.BankTransitNumber)
                .HasColumnName("BranchTransitNumber");

            builder.Property(x => x.SecondaryEmail)
                .HasColumnName("Secondary_Email");

            builder.Property(x => x.CanReceivePhysicalCopy)
                .HasColumnName("receive_Physical_Copy");

            builder.Property(x => x.IsSecondaryAdmin)
                .HasColumnName("IsSecondaryAdmin");

            builder.Property(x => x.ConsentIp)
                .HasColumnName("consent_ip");

            builder.Property(x => x.SensibillEmail)
                .HasColumnName("SensibillEmail");

            builder.Property(x => x.PhoneNumber)
                .HasColumnName("PhoneNumber");
        }
    }
}
