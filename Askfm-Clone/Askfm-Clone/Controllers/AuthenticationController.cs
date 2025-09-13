using Askfm_Clone.DTOs.Auth;
using Askfm_Clone.Repositories.Contracs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace Askfm_Clone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IUserAccountService _userAccountRepository;

        public AuthenticationController(IUserAccountService userAccountRepository)
        {
            _userAccountRepository = userAccountRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto user)
        {
            if (user == null)
                return BadRequest(AuthControllerResponseDto.ErrorResponse(
                    (int)HttpStatusCode.BadRequest, "User data cannot be null"));

            if (!ModelState.IsValid)
                return BadRequest(AuthControllerResponseDto.ErrorResponse(
                    (int)HttpStatusCode.BadRequest,
                    "Invalid registration data",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))));

            var response = await _userAccountRepository.RegisterAsync(user);

            if (!response.successFlag)
                return BadRequest(AuthControllerResponseDto.ErrorResponse(
                    (int)HttpStatusCode.BadRequest, response.Message));

            return Ok(AuthControllerResponseDto.SuccessResponse(
                (int)HttpStatusCode.OK, response.Message));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto user)
        {
            if (user == null)
                return BadRequest(AuthControllerResponseDto.ErrorResponse(
                    (int)HttpStatusCode.BadRequest, "User data cannot be null"));

            if (!ModelState.IsValid)
                return BadRequest(AuthControllerResponseDto.ErrorResponse(
                    (int)HttpStatusCode.BadRequest,
                    "Invalid login data",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))));

            var response = await _userAccountRepository.LoginAsync(user);

            if (!response.successFlag)
                return Unauthorized(AuthControllerResponseDto.ErrorResponse(
                    (int)HttpStatusCode.Unauthorized, response.Message));

            return Ok(AuthControllerResponseDto.SuccessResponse(
                (int)HttpStatusCode.OK, response.Message, response.Data));
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest(AuthControllerResponseDto.ErrorResponse(
                    (int)HttpStatusCode.BadRequest, "Refresh token cannot be empty"));

            var response = await _userAccountRepository.RefreshTokenAsync(refreshToken);

            if (!response.successFlag)
                return Unauthorized(AuthControllerResponseDto.ErrorResponse(
                    (int)HttpStatusCode.Unauthorized, response.Message));

            return Ok(AuthControllerResponseDto.SuccessResponse(
                (int)HttpStatusCode.OK, response.Message, response.Data));
        }
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] LogoutDto logoutDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(AuthControllerResponseDto.ErrorResponse(
                    (int)HttpStatusCode.BadRequest,
                    "Invalid data",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))));

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user authentication");
            }

            var response = await _userAccountRepository.LogoutAsync(userId , logoutDto.DeviceId);

            if (!response.successFlag)
                return BadRequest(AuthControllerResponseDto.ErrorResponse(
                    (int)HttpStatusCode.BadRequest, response.Message));

            return Ok(AuthControllerResponseDto.SuccessResponse(
                (int)HttpStatusCode.OK, response.Message));
        }

        [HttpPost("logout-all")]
        [Authorize]
        public async Task<IActionResult> LogoutAll()
        {
            if (!ModelState.IsValid)
                return BadRequest(AuthControllerResponseDto.ErrorResponse(
                    (int)HttpStatusCode.BadRequest,
                    "Invalid data",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))));

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user authentication");
            }

            var response = await _userAccountRepository.LogoutAllAsync(userId);

            if (!response.successFlag)
                return BadRequest(AuthControllerResponseDto.ErrorResponse(
                    (int)HttpStatusCode.BadRequest, response.Message));

            return Ok(AuthControllerResponseDto.SuccessResponse(
                (int)HttpStatusCode.OK, response.Message));
        }


    }
}