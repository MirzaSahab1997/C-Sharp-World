using IdentityVerificationService.Domain.Entities.dbo;
using IdentityVerificationService.Domain.Entities.IdVerification;
using IdentityVerificationService.Infrastructure.Configuration;
using LMSOnboardingApiV3.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityVerificationService.Infrastructure
{
    public class CaaryDbContext : DbContext
    {
        public CaaryDbContext(DbContextOptions<CaaryDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        public DbSet<RefDocumentType> RefDocumentTypes { get; set; }

        public DbSet<IDVerificationDocument> IDVerificationDocuments { get; set; }

        public DbSet<IDVerificationTransactionStatus> TrulioTransactionStatuses { get; set; }

        public DbSet<IDVerificationImage> TrulioImage { get; set; }

        public DbSet<IDVerificationKyc> IDVerificationKycs { get; set; }

        public DbSet<IDVerificationKyb> IDVerificationKybs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new RefDocumentTypeConfiguration());
            modelBuilder.ApplyConfiguration(new TruliooDocumentVerificationsConfiguration());
            modelBuilder.ApplyConfiguration(new TrulioTransactionStatusConfiguration());
            modelBuilder.ApplyConfiguration(new TruliooImageConfiguration());
            modelBuilder.ApplyConfiguration(new IDVerificationKycConfiguration());
            modelBuilder.ApplyConfiguration(new IDVerificationKybConfiguration());
        }
    }
}
