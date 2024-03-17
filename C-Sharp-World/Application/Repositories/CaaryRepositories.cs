using IdentityVerificationService.Infrastructure;

namespace IdentityVerificationService.Application.Repositories
{
    public class CaaryRepositories
    {
        private readonly CaaryDbContext _caaryDbContext;
        private IDVerificationRepository _idVerificationRepository;

        public CaaryRepositories(CaaryDbContext caaryDbContext)
        {
            _caaryDbContext = caaryDbContext ?? throw new ArgumentNullException($"{nameof(caaryDbContext)}");
        }

        public IDVerificationRepository IDVerificationRepository => _idVerificationRepository ?? (_idVerificationRepository = CreateIDVerificationRepository());

        private IDVerificationRepository CreateIDVerificationRepository()
        {
            return new IDVerificationRepository(_caaryDbContext);
        }
    }
}