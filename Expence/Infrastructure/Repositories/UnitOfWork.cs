using Expence.Infrastructure.Interface;
using Microsoft.EntityFrameworkCore;

namespace Expence.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ExpenceDbContext _expenceDbContext;
        private readonly ITransactionRepository _transactionRepository;
        private readonly  IUserRepository _userRepository;
        private readonly IEmailDomainInfoRepository _emailDomainInfoRepository;
        private bool _disposed = false;


        public UnitOfWork(
            ExpenceDbContext expenceDbContext,
            ITransactionRepository transactionRepository,
            IUserRepository userRepository,
            IEmailDomainInfoRepository emailDomainInfoRepository)
        {
           _expenceDbContext = expenceDbContext ?? throw new ArgumentNullException(nameof(expenceDbContext));
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository)); 
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _emailDomainInfoRepository = emailDomainInfoRepository ?? throw new ArgumentNullException(nameof(emailDomainInfoRepository));
        }


        public ITransactionRepository Transactions => _transactionRepository;

        public IUserRepository Users => _userRepository;

        public IEmailDomainInfoRepository EmailDomainInfo => _emailDomainInfoRepository;
       

        public async Task<int> SaveAsync()
        {
            return await _expenceDbContext.SaveChangesAsync();
        }
    }
}
