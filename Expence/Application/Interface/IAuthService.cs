using Expence.Domain.DTOs;

namespace Expence.Application.Interface
{
    public interface IAuthService
    {
        Task<BaseResponse<AuthResponseDto>> Register(RegisterDTo registerDto);
        Task<BaseResponse<AuthResponseDto>> Login(LoginDTO loginDto);
        Task<BaseResponse<AuthResponseDto>> Refresh(string token, string refreshToken);

    }
}
