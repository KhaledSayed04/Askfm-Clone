using Askfm_Clone.Repositories.Contracs;
using Base_Library.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Askfm_Clone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IUserAccountRepository _userAccountRepository;

        public AuthenticationController(IUserAccountRepository userAccountRepository)
        {
            _userAccountRepository = userAccountRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto user)
        {
            if (user == null)
            {
                return BadRequest(new
                {
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = "User data cannot be null"
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = "Invalid registration data",
                    // to have the specific error
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var response = await _userAccountRepository.RegisterAsync(user);

            if (!response.successFlag)
            {
                return BadRequest(new
                {
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = response.Message
                });
            }

            return Ok(new
            {
                statusCode = (int)HttpStatusCode.OK,
                message = response.Message
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto user)
        {
            if (user == null)
            {
                return BadRequest(new
                {
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = "User data cannot be null"
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = "Invalid login data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var response = await _userAccountRepository.LoginAsync(user);

            if (!response.successFlag)
            {
                return Unauthorized(new
                {
                    statusCode = (int)HttpStatusCode.Unauthorized,
                    message = response.Message
                });
            }

            return Ok(new
            {
                statusCode = (int)HttpStatusCode.OK,
                message = response.Message,
                data = response.Data // contains AccessToken & RefreshToken
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest(new
                {
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = "Refresh token cannot be empty"
                });
            }

            var response = await _userAccountRepository.RefreshTokenAsync(refreshToken);

            if (!response.successFlag)
            {
                return Unauthorized(new
                {
                    statusCode = (int)HttpStatusCode.Unauthorized,
                    message = response.Message
                });
            }

            return Ok(new
            {
                statusCode = (int)HttpStatusCode.OK,
                message = response.Message,
                data = response.Data // contains new AccessToken & RefreshToken
            });
        }
    }
}