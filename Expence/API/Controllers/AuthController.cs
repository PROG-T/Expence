using Asp.Versioning;
using Expence.Application.Interface;
using Expence.Domain.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Expence.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [EnableRateLimiting("auth")]
    public class AuthController : ControllerBase 
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user with email and password.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTo request) 
        { 
            var response = await _authService.Register(request);
            if (response.Status == false) return BadRequest(response);
            return Ok(response);
        }

        /// <summary>
        /// Authenticates a user and returns JWT and refresh tokens.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO request)
        {
            var response = await _authService.Login(request);
            if (response.Status == false) return BadRequest(response);
            return Ok(response);
        }

        // <summary>
        /// Refreshes an expired JWT token using a valid refresh token.
        /// </summary>
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            var response = await _authService.Refresh(request.Token, request.RefreshToken);
            if (response.Status == false) return BadRequest(response);
            return Ok(response);
        }

        /// <summary>
        /// Verifies a user's email address using a verification token.
        /// </summary>
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
        {
            var response = await _authService.VerifyEmailAsync(request);
            if (!response.Status)
                return BadRequest(response);
            return Ok(response);
        }

        /// <summary>
        /// Resends the email verification link to the user.
        /// </summary>
        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerificationEmail([FromBody] ResendVerificationEmailRequest request)
        {
            var response = await _authService.ResendVerificationEmailAsync(request);
            return Ok(response);
        }

        /// <summary>
        /// Initiates the password reset process by sending a reset link to the user's email.
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var response = await _authService.ForgotPasswordAsync(request);
            return Ok(response);
        }

        /// <summary>
        /// Resets the user's password using a valid reset token.
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var response = await _authService.ResetPasswordAsync(request);
            if (response.Status == false) return BadRequest(response);
            return Ok(response);
        }
    }
}
