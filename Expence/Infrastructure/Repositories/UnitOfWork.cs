using Expence.Infrastructure.Interface;
using Microsoft.EntityFrameworkCore;

namespace Expence.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ExpenceDbContext _expenceDbContext;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEmailDomainInfoRepository _emailDomainInfoRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
        private readonly IEmailVerificationTokenRepository _emailVerificationTokenRepository;



        public UnitOfWork(
            ExpenceDbContext expenceDbContext,
            ITransactionRepository transactionRepository,
            IUserRepository userRepository,
            IEmailDomainInfoRepository emailDomainInfoRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IPasswordResetTokenRepository passwordResetTokenRepository,
            IEmailVerificationTokenRepository emailVerificationTokenRepository)
        {
            _expenceDbContext = expenceDbContext ?? throw new ArgumentNullException(nameof(expenceDbContext));
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _emailDomainInfoRepository = emailDomainInfoRepository ?? throw new ArgumentNullException(nameof(emailDomainInfoRepository));
            _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
            _passwordResetTokenRepository = passwordResetTokenRepository ?? throw new ArgumentNullException(nameof(passwordResetTokenRepository));
            _emailVerificationTokenRepository = emailVerificationTokenRepository ?? throw new ArgumentNullException(nameof(emailVerificationTokenRepository));
        }


        public ITransactionRepository Transactions => _transactionRepository;

        public IUserRepository Users => _userRepository;

        public IEmailDomainInfoRepository EmailDomainInfo => _emailDomainInfoRepository;
        public IRefreshTokenRepository RefreshTokens => _refreshTokenRepository;
        public IPasswordResetTokenRepository PasswordResetTokens => _passwordResetTokenRepository;
        public IEmailVerificationTokenRepository EmailVerificationTokens => _emailVerificationTokenRepository;
        public async Task<int> SaveAsync()
        {
            return await _expenceDbContext.SaveChangesAsync();
        }
    }
}
