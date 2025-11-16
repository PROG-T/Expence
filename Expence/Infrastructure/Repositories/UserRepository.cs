using Expence.Domain.DTOs;
using Expence.Domain.Models;
using Expence.Infrastructure.Interface;
using Microsoft.EntityFrameworkCore;

namespace Expence.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ExpenceDbContext _context;
        public UserRepository(ExpenceDbContext context)
        {
             _context = context;
        }
        public async Task<UserDto> AddUserAsync(User user)
        {
           await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return new UserDto
            {
                Email = user.Email
            };
        }

        public async Task<UserDto> GetUserByEmailAsync(string email)
        {
           var response = await _context.Users
                .Where(u => u.Email == email)
                .Select(u => new UserDto
                {
                    Email = u.Email
                })
                .FirstOrDefaultAsync();
            return response?? new UserDto { };
        }

    }
}
