using Expence.Domain.DTOs;

namespace Expence.Application.Interface
{
    public interface IUserContextService
    {
        BaseResponse<string> GetUserId();
        BaseResponse<string> GetUserEmail();
    }
}
