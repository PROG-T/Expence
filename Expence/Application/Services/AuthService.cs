using Expence.Application.Interface;
using Expence.Domain.DTOs;
using Expence.Domain.Models;
using Expence.Infrastructure.Interface;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Expence.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IJwtService _jwtService;
        private readonly DisposableEmailService _emailService;
        private readonly ISmtpEmailService _smtpEmailService;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<AuthService> _logger;



        public AuthService(IJwtService jwtService, IUnitOfWork uow, DisposableEmailService emailService, ILogger<AuthService> logger, ISmtpEmailService smtpEmailService)
        {
            _jwtService = jwtService;
            _uow = uow;
            _emailService = emailService;
            _logger = logger;
            _smtpEmailService = smtpEmailService;
        }
        public async Task<BaseResponse<AuthResponseDto>> Login(LoginDTO loginDto)
        {
            _logger.LogInformation("Login attempt for email: {Email}", loginDto.Email);

            var user = await _uow.Users.GetUserByEmailAsync(loginDto.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: User not found. Email: {Email}", loginDto.Email);
                return new BaseResponse<AuthResponseDto>(false, "User does not exist, pls register");
            }

            // Check if email is verified
            if (!user.IsEmailVerified)
            {
                _logger.LogWarning("Login failed: Email not verified. Email: {Email}", loginDto.Email);
                return new BaseResponse<AuthResponseDto>(false, "Please verify your email before logging in. Check your inbox for verification email.");
            }

            bool passwordMatch = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);
            if (!passwordMatch)
            {
                _logger.LogWarning("Login failed: Invalid password. Email: {Email}", loginDto.Email);
                return new BaseResponse<AuthResponseDto>(false, "Invalid password");
            }

            var token = _jwtService.GenerateToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken(user);

            // Store refresh token in database
            await _uow.RefreshTokens.AddRefreshTokenAsync(refreshToken);
            await _uow.SaveAsync();

            _logger.LogInformation("Login successful. UserId: {UserId}, Email: {Email}", user.Id, loginDto.Email);

            var response = new AuthResponseDto { Expiration = DateTime.Now.AddMinutes(30), Token = token, RefreshToken = refreshToken.Token };

            return new BaseResponse<AuthResponseDto>(true, "Login successful", response);
        }

        public async Task<BaseResponse<AuthResponseDto>> Refresh(string token, string refreshToken)
        {
            _logger.LogInformation("Token refresh requested");

            var storedRefreshToken = await _uow.RefreshTokens.GetRefreshTokenAsync(refreshToken);
            if (storedRefreshToken == null)
            {
                _logger.LogWarning("Token refresh failed: Invalid or expired refresh token");
                return new BaseResponse<AuthResponseDto>(false, "Invalid or expired refresh token");
            }

            var principal = _jwtService.GetClaimsPrincipalFromExpiredToken(token);
            var email = principal.FindFirstValue(ClaimTypes.Email);

            var user = await _uow.Users.GetUserByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("Token refresh failed: User not found. Email: {Email}", email);
                return new BaseResponse<AuthResponseDto>(false, "user not found");
            }

            await _uow.RefreshTokens.RevokeRefreshTokenAsync(refreshToken);

            var newJwt = _jwtService.GenerateToken(user);
            var newRefresh = _jwtService.GenerateRefreshToken(user);

            await _uow.RefreshTokens.AddRefreshTokenAsync(newRefresh);
            await _uow.SaveAsync();

            _logger.LogInformation("Token refreshed successfully. UserId: {UserId}, Email: {Email}", user.Id, email);

            return new BaseResponse<AuthResponseDto>(true, "Token refreshed successfully",
                new AuthResponseDto
                {
                    Token = newJwt,
                    RefreshToken = newRefresh.Token,
                    Expiration = DateTime.UtcNow.AddMinutes(30)
                }
            );
        }

        public async Task<BaseResponse<AuthResponseDto>> Register(RegisterDTo registerDto)
        {
            _logger.LogInformation("Registration attempt for email: {Email}", registerDto.Email);

            var response = await _emailService.IsDisposableEmailAsync(registerDto.Email);
            if (response.Status)
            {
                _logger.LogWarning("Registration failed: Disposable email. Email: {Email}", registerDto.Email);
                return new BaseResponse<AuthResponseDto>(false, "Disposable email addresses are not allowed.");
            }

            var existing = await _uow.Users.GetUserByEmailAsync(registerDto.Email);
            if (existing != null)
            {
                _logger.LogWarning("Registration failed: User already exists. Email: {Email}", registerDto.Email);
                return new BaseResponse<AuthResponseDto>(false, "user already exists");
            }
            //check if pass word is valid(using regex)

            var passwordRegex = new Regex(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^\w\s]).{8,}$");
            if (!passwordRegex.IsMatch(registerDto.Password))
            {
                _logger.LogWarning("Registration failed: Invalid password format. Email: {Email}", registerDto.Email);
                return new BaseResponse<AuthResponseDto>(false, "Password must contain an uppercase, number, symbol, and be at least 8 characters.");
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);
            var user = new User
            {
                Email = registerDto.Email.Trim(),
                PasswordHash = hashedPassword
            };

            var addedUser = await _uow.Users.AddUserAsync(user);
            await _uow.SaveAsync();

            _logger.LogInformation("User created. UserId: {UserId}, Email: {Email}", addedUser.Id, registerDto.Email);

            // Generate verification token
            var verificationToken = GenerateVerificationToken();
            var emailVerificationToken = new EmailVerificationToken
            {
                UserId = addedUser.Id,
                Token = verificationToken,
                ExpiryDate = DateTime.UtcNow.AddHours(24),
                IsUsed = false
            };

            await _uow.EmailVerificationTokens.AddTokenAsync(emailVerificationToken);
            await _uow.SaveAsync();

            // Send verification email (non-blocking)
            _ = _smtpEmailService.SendEmailVerificationEmailAsync(user.Email, verificationToken);

            _logger.LogInformation("User registered successfully. Verification email sent to {Email}", user.Email);

            return new BaseResponse<AuthResponseDto>(true,
                "Registration successful! Please check your email to verify your account. Verification link expires in 24 hours.");
        }
        public async Task<BaseResponse<bool>> VerifyEmailAsync(VerifyEmailRequest request)
        {
            try
            {
                _logger.LogInformation("Email verification attempt for: {Email}", request.Email);

                var user = await _uow.Users.GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    _logger.LogWarning("Email verification failed: User not found. Email: {Email}", request.Email);
                    return new BaseResponse<bool>(false, "User not found");
                }

                if (user.IsEmailVerified)
                {
                    _logger.LogInformation("Email verification: Email already verified. Email: {Email}", request.Email);
                    return new BaseResponse<bool>(true, "Email already verified");
                }

                var verificationToken = await _uow.EmailVerificationTokens.GetValidTokenAsync(request.Token, user.Id);
                if (verificationToken == null)
                {
                    _logger.LogWarning("Email verification failed: Invalid or expired token. Email: {Email}", request.Email);
                    return new BaseResponse<bool>(false, "Invalid or expired verification token");
                }

                // Mark user email as verified
                user.IsEmailVerified = true;
                user.EmailVerifiedAt = DateTime.UtcNow;

                // Mark token as used
                verificationToken.IsUsed = true;

                _uow.Users.UpdateUserAsync(user);
                await _uow.EmailVerificationTokens.UpdateTokenAsync(verificationToken);
                await _uow.SaveAsync();

                _logger.LogInformation("Email verified successfully for user: {Email}", user.Email);
                return new BaseResponse<bool>(true, "Email verified successfully! You can now log in.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying email");
                return new BaseResponse<bool>(false, "An error occurred while verifying your email");
            }
        }

        public async Task<BaseResponse<bool>> ResendVerificationEmailAsync(ResendVerificationEmailRequest request)
        {
            try
            {
                _logger.LogInformation("Resend verification email requested for: {Email}", request.Email);

                var user = await _uow.Users.GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    _logger.LogDebug("Resend verification: User not found (security masked). Email: {Email}", request.Email);
                    // Don't reveal if user exists for security
                    return new BaseResponse<bool>(true, "If the email exists, a verification email will be sent");
                }

                if (user.IsEmailVerified)
                {
                    _logger.LogInformation("Resend verification: Email already verified. Email: {Email}", request.Email);
                    return new BaseResponse<bool>(true, "Email is already verified. You can log in.");
                }

                // Invalidate old tokens
                await _uow.EmailVerificationTokens.InvalidateAllTokensAsync(user.Id);

                // Generate new verification token
                var verificationToken = GenerateVerificationToken();
                var emailVerificationToken = new EmailVerificationToken
                {
                    UserId = user.Id,
                    Token = verificationToken,
                    ExpiryDate = DateTime.UtcNow.AddHours(24),
                    IsUsed = false
                };

                await _uow.EmailVerificationTokens.AddTokenAsync(emailVerificationToken);
                await _uow.SaveAsync();

                // Send verification email (non-blocking)
                _ = _smtpEmailService.SendEmailVerificationEmailAsync(user.Email, verificationToken);

                _logger.LogInformation("Verification email resent to {Email}", user.Email);
                return new BaseResponse<bool>(true, "If the email exists, a verification email will be sent");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending verification email");
                return new BaseResponse<bool>(false, "An error occurred while resending verification email");
            }
        }

        public async Task<BaseResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            try
            {
                _logger.LogInformation("Password reset requested for: {Email}", request.Email);

                var user = await _uow.Users.GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    _logger.LogDebug("Password reset: User not found (security masked). Email: {Email}", request.Email);
                    // Don't reveal if email exists or not for security
                    return new BaseResponse<bool>(true, "If the email exists, a password reset link will be sent");
                }

                // Invalidate all previous tokens
                await _uow.PasswordResetTokens.InvalidateAllTokensAsync(user.Id);

                // Generate reset token
                var resetToken = GenerateResetToken();
                var passwordResetToken = new PasswordResetToken
                {
                    UserId = user.Id,
                    Token = resetToken,
                    ExpiryDate = DateTime.UtcNow.AddHours(1),
                    IsUsed = false
                };

                await _uow.PasswordResetTokens.AddTokenAsync(passwordResetToken);
                await _uow.SaveAsync();

                _ = _smtpEmailService.SendPasswordResetEmailAsync(user.Email, resetToken);
                _logger.LogInformation("Password reset email sent to user: {Email}", user.Email);                // Example: await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken);

                return new BaseResponse<bool>(true, "If the email exists, a password reset link will be sent");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ForgotPasswordAsync");
                return new BaseResponse<bool>(false, "An error occurred while processing your request");
            }

     }

        public async Task<BaseResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request)
        {
            try
            {
                _logger.LogInformation("Password reset attempt for email: {Email}", request.Email);

                if (request.NewPassword != request.ConfirmPassword)
                {
                    _logger.LogWarning("Password reset failed: Passwords do not match. Email: {Email}", request.Email);
                    return new BaseResponse<bool>(false, "Passwords do not match");
                }

                var passwordRegex = new Regex(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^\w\s]).{8,}$");
                if (!passwordRegex.IsMatch(request.NewPassword))
                {
                    _logger.LogWarning("Password reset failed: Invalid password format. Email: {Email}", request.Email);
                    return new BaseResponse<bool>(false, "Password must contain an uppercase, number, symbol, and be at least 8 characters.");
                }

                var user = await _uow.Users.GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    _logger.LogWarning("Password reset failed: User not found. Email: {Email}", request.Email);
                    return new BaseResponse<bool>(false, "User not found");
                }

                var resetToken = await _uow.PasswordResetTokens.GetValidTokenAsync(request.Token, user.Id);
                if (resetToken == null)
                {
                    _logger.LogWarning("Password reset failed: Invalid or expired token. Email: {Email}", request.Email);
                    return new BaseResponse<bool>(false, "Invalid or expired reset token");
                }

                // Update password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

                // Mark token as used
                resetToken.IsUsed = true;

                // Invalidate all other tokens
                await _uow.PasswordResetTokens.InvalidateAllTokensAsync(user.Id);
                resetToken.IsUsed = true; // Ensure this token is marked as used

                _uow.Users.UpdateUserAsync(user);
                await _uow.PasswordResetTokens.UpdateTokenAsync(resetToken);
                await _uow.SaveAsync();

                _logger.LogInformation("Password reset successful for user: {Email}", user.Email);
                return new BaseResponse<bool>(true, "Password reset successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ResetPasswordAsync");
                return new BaseResponse<bool>(false, "An error occurred while resetting your password");
            }
        }
        private string GenerateVerificationToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
        private string GenerateResetToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

    }
}
