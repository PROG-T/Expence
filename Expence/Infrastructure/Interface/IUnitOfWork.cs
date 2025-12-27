namespace Expence.Infrastructure.Interface
{
    public interface IUnitOfWork
    {
        ITransactionRepository Transactions { get; }
        IUserRepository Users { get; }
        IEmailDomainInfoRepository EmailDomainInfo { get; }

        Task<int> SaveAsync();

    }
}
