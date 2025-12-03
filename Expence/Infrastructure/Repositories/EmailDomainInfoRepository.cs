using Expence.Domain.Models;
using Expence.Infrastructure.Interface;
using Microsoft.EntityFrameworkCore;

namespace Expence.Infrastructure.Repositories
{
    public class EmailDomainInfoRepository : IEmailDomainInfoRepository
    {
        private readonly ExpenceDbContext _context;
        public EmailDomainInfoRepository(ExpenceDbContext context)
        {
            _context = context;
        }
        public async Task<EmailDomainInfo> AddDomainNameAsync(EmailDomainInfo emailDomainInfo)
        {
            await _context.EmailDomainInfo.AddAsync(emailDomainInfo);
            await _context.SaveChangesAsync();
            return emailDomainInfo;
        }

        public async Task<EmailDomainInfo> GetDomainNameAsync(string domainName)
        {
            return await _context.EmailDomainInfo.FirstOrDefaultAsync(x => x.Domain == domainName);
        }
    }
}
