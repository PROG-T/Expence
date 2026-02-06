namespace Expence.Infrastructure.Interface
{
    public interface IUnitOfWork
    {
        ITransactionRepository Transactions { get; }
        IUserRepository Users { get; }
        IEmailDomainInfoRepository EmailDomainInfo { get; }
        IRefreshTokenRepository RefreshTokens { get; }
        IPasswordResetTokenRepository PasswordResetTokens { get; }
        IEmailVerificationTokenRepository EmailVerificationTokens { get; }



        Task<int> SaveAsync();

    }
}
