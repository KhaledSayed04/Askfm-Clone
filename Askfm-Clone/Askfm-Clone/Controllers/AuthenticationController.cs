using Askfm_Clone.Repositories.Contracs;
using Base_Library.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Askfm_Clone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController(IUserAccountRepository userAccountRepository) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(Register user)
        {
            if (user is null)
            {
                return BadRequest("User cannot be empty.");
            }
            var response = await userAccountRepository.RegisterAsync(user);
            if (!response.Flag)
            {
                return BadRequest(response.Message);
            }
            return Ok(response);
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Base_Library.DTOs.Login user)
        {
            if (user is null)
            {
                return BadRequest("User cannot be empty.");
            }
            var response = await userAccountRepository.SignInAsync(user);
            if (!response.Flag)
            {
                return Unauthorized(response.Message);
            }
            return Ok(response);
        }
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshToken refreshToken)
        {
            if (refreshToken is null)
            {
                return BadRequest("Refresh token cannot be empty.");
            }
            var response = await userAccountRepository.RefreshTokenAsync(refreshToken);
            if (!response.Flag)
            {
                return Unauthorized(response.Message);
            }
            return Ok(response);
        }

    }
}
