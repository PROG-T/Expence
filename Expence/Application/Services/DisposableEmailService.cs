using Expence.Domain.Constants;
using Expence.Domain.DTOs;
using Expence.Domain.Models;
using Expence.Infrastructure.Interface;
using System;
using System.Net.Http.Json;

namespace Expence.Application.Services
{
    public class DisposableEmailService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DisposableEmailService> _logger;

        public DisposableEmailService(IUnitOfWork unitOfWork, ILogger<DisposableEmailService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<BaseResponse<bool>> CheckDomainWithExternalApi(string email)
        {
            _logger.LogInformation("Checking domain with external API for email: {Email}", email);

            using var http = new HttpClient();
            var response = await http.GetFromJsonAsync<DisposableApiResponse>($"https://disposable.debounce.io/?email={email}");
          
            if (response == null)
            {
                _logger.LogWarning("External API returned null response for email: {Email}", email);
                return new BaseResponse<bool>(false, "API response was null");
            }

            var isDisposable = response.Disposable == "true";

            _logger.LogInformation("External API check completed for email: {Email}, IsDisposable: {IsDisposable}",
                email, isDisposable);

            return new BaseResponse<bool>(isDisposable, "");
        }

        public async Task<BaseResponse<bool>> IsDisposableEmailAsync(string email)
        {
            _logger.LogInformation("Checking if email is disposable: {Email}", email);

            var domain = GetDomainFromEmail(email).Data;
            _logger.LogDebug("Extracted domain from email: {Domain}", domain);

            var existing = await _unitOfWork.EmailDomainInfo.GetDomainNameAsync(domain);
            if (existing != null)
            {
                _logger.LogInformation("Domain check found in cache. Domain: {Domain}, IsDisposable: {IsDisposable}",
                        domain, existing.IsDisposable);
                return new BaseResponse<bool>(existing.IsDisposable, "");
            }

            _logger.LogDebug("Domain not in cache, calling external API. Domain: {Domain}", domain);

            var isDisposable = await CheckDomainWithExternalApi(email);
            if (!isDisposable.Status)
            {
                _logger.LogWarning("External API check failed for domain: {Domain}", domain);
                return isDisposable;
            }


            var domainInfo = new EmailDomainInfo
            {
                CheckedAt = DateTimeConstants.CurrentWestAfricanTime,
                Domain = domain,
                IsDisposable = isDisposable.Status
            };

            await _unitOfWork.EmailDomainInfo.AddDomainNameAsync(domainInfo);
            await _unitOfWork.SaveAsync();

            _logger.LogInformation("Domain check cached. Domain: {Domain}, IsDisposable: {IsDisposable}, CheckedAt: {CheckedAt}",
                   domain, domainInfo.IsDisposable, domainInfo.CheckedAt);

            return new BaseResponse<bool>(isDisposable.Status, "");

        }

        private BaseResponse<string> GetDomainFromEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            {
                _logger.LogWarning("Invalid email format: {Email}", email);
                return new BaseResponse<string>(false, "email is invalid");
            }

            var domain = email.Split("@").Last().Trim().ToLower();
            _logger.LogDebug("Domain extracted from email: {Domain}", domain);

            return new BaseResponse<string>(true, domain);
        }
    }
}
