using Expence.Domain.DTOs;

namespace Expence.Application.Interface
{
    public interface IAuthService
    {
        Task<BaseResponse<AuthResponseDto>> Register(RegisterDTo registerDto);
        Task<BaseResponse<AuthResponseDto>> Login(LoginDTO loginDto);
        Task<BaseResponse<AuthResponseDto>> Refresh(string token, string refreshToken);
        Task<BaseResponse<bool>> VerifyEmailAsync(VerifyEmailRequest request);
        Task<BaseResponse<bool>> ResendVerificationEmailAsync(ResendVerificationEmailRequest request);
        Task<BaseResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<BaseResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request);

    }
}
