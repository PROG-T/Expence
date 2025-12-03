using Expence.Domain.Constants;
using Expence.Domain.DTOs;
using Expence.Domain.Models;
using Expence.Infrastructure.Interface;
using System.Net.Http.Json;

namespace Expence.Application.Services
{
    public class EmailService
    {
        private readonly IEmailDomainInfoRepository _emailDomainInfoRepository;
        public EmailService(IEmailDomainInfoRepository emailDomainInfoRepository)
        {
            _emailDomainInfoRepository = emailDomainInfoRepository;
        }
        public async Task<BaseResponse<bool>> CheckDomainWithExternalApi(string email) 
        {
            using var http = new HttpClient();
            var response = await http.GetFromJsonAsync<DisposableApiResponse>($"https://disposable.debounce.io/?email={email}");
            return new BaseResponse<bool>(response?.Disposable == "true", "");
        }

        public async Task<BaseResponse<bool>> IsDisposableEmailAsync(string email)
        {
            var domain = GetDomainFromEmail(email);
            var existing = await _emailDomainInfoRepository.GetDomainNameAsync(domain);
            if (existing != null)
                return new BaseResponse<bool>(existing.IsDisposable,"");

            var isDisposable = await CheckDomainWithExternalApi(email);

            var domainInfo = new EmailDomainInfo
            {
                CheckedAt = DateTimeConstants.CurrentWestAfricanTime,
                Domain = domain,
                IsDisposable = isDisposable.Status
            };

            await _emailDomainInfoRepository.AddDomainNameAsync(domainInfo);
            return new BaseResponse<bool>(isDisposable.Status, "");

        }

        public string GetDomainFromEmail(string email)
        { 
            return email.Split("@").Last().Trim().ToLower();
        }
    }
}
