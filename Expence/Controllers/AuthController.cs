using Expence.Application.Interface;
using Expence.Domain.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Expence.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class AuthController : ControllerBase 
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTo request) 
        { 
            if(!ModelState.IsValid) return BadRequest(ModelState);

            var response = await _authService.Register(request);
            if (response.Status == false) return BadRequest(response);
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var response = await _authService.Login(request);
            if (response.Status == false) return BadRequest(response);
            return Ok(response);
        }
    }
}
