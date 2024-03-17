using IdentityVerificationService.Domain.Entities.IdVerification;
using IdentityVerificationService.Infrastructure;
using LMSOnboardingApiV3.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityVerificationService.Application.Repositories
{
    public class IDVerificationRepository : BaseRepository
    {
        public IDVerificationRepository(CaaryDbContext caaryDbContext) : base(caaryDbContext) { }

        public async Task<IDVerificationImage> GetTruliooImageAsync(long? userId, CancellationToken cancellationToken = default)
            => await _caaryDbContext.TrulioImage.Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync(cancellationToken);

        public async Task<List<RefDocumentType>> GetDocumentTypeAsync(CancellationToken cancellationToken = default)
             => await _caaryDbContext.RefDocumentTypes.ToListAsync(cancellationToken);

        public async Task<IDVerificationDocument> GetDocumentVerificationAsync(long? clientId, CancellationToken cancellationToken = default)
             => await _caaryDbContext.IDVerificationDocuments.Where(x => x.UserId == clientId).OrderByDescending(x => x.Id).FirstOrDefaultAsync(cancellationToken);

        public async Task<IDVerificationDocument> GetDocumentVerificationByTransactionIdAsync(Guid? transactionId, CancellationToken cancellationToken = default)
             => await _caaryDbContext.IDVerificationDocuments.Where(x => x.TransactionId == transactionId).OrderByDescending(x => x.Id).FirstOrDefaultAsync(cancellationToken);

        public async Task<IDVerificationImage> GetTruliooImageByTransactionIdAsync(Guid? transactionId, CancellationToken cancellationToken = default)
            => await _caaryDbContext.TrulioImage.Where(x => x.TransactionId == transactionId).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync(cancellationToken);

        public async Task<IDVerificationKyc> GetIDVerificationKycAsync(long? userId, int personalRoleId, CancellationToken cancellationToken = default)
            => await _caaryDbContext.IDVerificationKycs.Where(x => x.ApplicationId == userId && x.PersonalRoleId == personalRoleId).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync(cancellationToken);

        public async Task<IDVerificationKyc> GetIDVerificationKycAsync(long? userId, int personalRoleId, int leaderId, CancellationToken cancellationToken = default)
            => await _caaryDbContext.IDVerificationKycs.Where(x => x.ApplicationId == userId && x.PersonalRoleId == personalRoleId && x.PersonalRecordId == leaderId).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync(cancellationToken);

        public async Task<List<IDVerificationKyc>> GetIDVerificationKycSigningAsync(long? userId, int personalRoleId, CancellationToken cancellationToken = default)
            => await _caaryDbContext.IDVerificationKycs.Where(x => x.ApplicationId == userId && x.PersonalRecordId == personalRoleId).OrderByDescending(x => x.CreatedDate).ToListAsync(cancellationToken);

        public async Task<IDVerificationKyb> GetIDVerificationKybAsync(long? userId, CancellationToken cancellationToken = default)
            => await _caaryDbContext.IDVerificationKybs.Where(x => x.ApplicationId == userId).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync(cancellationToken);

        public async Task<IDVerificationKyb> GetIDVerificationKybAsync(Guid? transactionId, CancellationToken cancellationToken = default)
            => await _caaryDbContext.IDVerificationKybs.Where(x => x.TransactionId == transactionId).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync(cancellationToken);

        public async Task<List<IDVerificationKyc>> GetIDVerificationKycOwnerShipAsync(long? userId, int personalRecordId, CancellationToken cancellationToken = default)
            => await _caaryDbContext.IDVerificationKycs.Where(x => x.ApplicationId == userId && x.PersonalRecordId == personalRecordId).OrderByDescending(x => x.CreatedDate).ToListAsync(cancellationToken);

        public async Task<IDVerificationKyc> GetIDVerificationKycOwnerShipAsync(long? userId, int personalRoleId, int ownerShipId, CancellationToken cancellationToken = default)
            => await _caaryDbContext.IDVerificationKycs.Where(x => x.ApplicationId == userId && x.PersonalRoleId == personalRoleId && x.PersonalRecordId == ownerShipId).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync(cancellationToken);
    }
}