using Expence.Domain.Models;

namespace Expence.Infrastructure.Interface
{
    public interface IEmailDomainInfoRepository
    {
        Task<EmailDomainInfo> GetDomainNameAsync(string domainName);
        Task<EmailDomainInfo> AddDomainNameAsync(EmailDomainInfo emailDomainInfo);
    }
}
