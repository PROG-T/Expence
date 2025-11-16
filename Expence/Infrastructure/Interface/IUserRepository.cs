using Expence.Domain.DTOs;
using Expence.Domain.Models;

namespace Expence.Infrastructure.Interface
{
    public interface IUserRepository
    {
        Task<UserDto> GetUserByEmailAsync(string email);
        Task<UserDto> AddUserAsync(User user);
    }
}
