using Expence.Application.Interface;
using Expence.Domain.DTOs;
using Expence.Domain.Models;
using Expence.Infrastructure.Interface;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Expence.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IJwtService _jwtService;
        private readonly IUserRepository _userRepository;
        private readonly EmailService _emailService;
        public AuthService(IJwtService jwtService, IUserRepository userRepository, EmailService emailService)
        {
            _jwtService = jwtService;
            _userRepository = userRepository;
            _emailService = emailService;
        }
        public async Task<BaseResponse<AuthResponseDto>> Login(LoginDTO loginDto)
        {
            var user = await _userRepository.GetUserByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return new BaseResponse<AuthResponseDto>(false, "User does not exist, pls register");
            }
            bool passwordMatch = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);
            if (!passwordMatch)
            {
                return new BaseResponse<AuthResponseDto>(false, "Invalid password");
            }
            var token = _jwtService.GenerateToken(user);
            var response = new AuthResponseDto { Expiration = DateTime.Now.AddMinutes(30), Token = token, RefreshToken = _jwtService.GenerateRefreshToken(user) };

            return new BaseResponse<AuthResponseDto>(true, "Login successful", response);
        }

        public async Task<BaseResponse<AuthResponseDto>> Refresh(string token, string refreshToken)
        {
            var principal = _jwtService.GetClaimsPrincipalFromExpiredToken(token);
            var email = principal.FindFirstValue(ClaimTypes.Email);

            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null) { return new BaseResponse<AuthResponseDto>(false, "user not found"); }

            var newJwt = _jwtService.GenerateToken(user);
            var newRefresh = _jwtService.GenerateRefreshToken(user);

            return new BaseResponse<AuthResponseDto>(true, "",
                new AuthResponseDto
                {
                    Token = newJwt,
                    RefreshToken = newRefresh,
                    Expiration = DateTime.UtcNow.AddMinutes(03)
                }
            );
        }

        public async Task<BaseResponse<AuthResponseDto>> Register(RegisterDTo registerDto)
        {
            var response = await _emailService.IsDisposableEmailAsync(registerDto.Email);
            if (response.Status) 
            {
                return new BaseResponse<AuthResponseDto>(false, "Disposable email addresses are not allowed.");
            }

            var existing = await _userRepository.GetUserByEmailAsync(registerDto.Email);
            if (existing != null)
            {
                return new BaseResponse<AuthResponseDto>(false, "user already exists");
            }
            //check if pass word is valid(using regex)

            var passwordRegex = new Regex(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^\w\s]).{8,}$");
            if (!passwordRegex.IsMatch(registerDto.Password))
            {
                return new BaseResponse<AuthResponseDto>(false, "Password must contain an uppercase, number, symbol, and be at least 8 characters.");
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);
            var user = new User
            {
                Email = registerDto.Email.Trim(),
                PasswordHash = hashedPassword
            };

            var addedUser = await _userRepository.AddUserAsync(user);

            var token = _jwtService.GenerateToken(user);

            var result = new AuthResponseDto
            {
                Token = token,
                RefreshToken = _jwtService.GenerateRefreshToken(user),
                Expiration = DateTime.Now.AddMinutes(30)
            };

            return new BaseResponse<AuthResponseDto>(true, "successfully registered", result);
        }
    }
}
