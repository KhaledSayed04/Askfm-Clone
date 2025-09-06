using Askfm_Clone.DTOs;
using Askfm_Clone.Repositories.Contracs;
using Base_Library.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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
        public async Task<IActionResult> Logout([FromBody] LogoutDto logoutDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(AuthControllerResponseDto.ErrorResponse(
                    (int)HttpStatusCode.BadRequest,
                    "Invalid data",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))));

            var response = await _userAccountRepository.LogoutAsync(logoutDto.UserId , logoutDto.DeviceId);

            if (!response.successFlag)
                return BadRequest(AuthControllerResponseDto.ErrorResponse(
                    (int)HttpStatusCode.BadRequest, response.Message));

            return Ok(AuthControllerResponseDto.SuccessResponse(
                (int)HttpStatusCode.OK, response.Message));
        }

        [HttpPost("logout-all")]
        public async Task<IActionResult> LogoutAll([FromBody] LogoutAllDto logoutDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(AuthControllerResponseDto.ErrorResponse(
                    (int)HttpStatusCode.BadRequest,
                    "Invalid data",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))));

            var response = await _userAccountRepository.LogoutAllAsync(logoutDto.UserId);

            if (!response.successFlag)
                return BadRequest(AuthControllerResponseDto.ErrorResponse(
                    (int)HttpStatusCode.BadRequest, response.Message));

            return Ok(AuthControllerResponseDto.SuccessResponse(
                (int)HttpStatusCode.OK, response.Message));
        }


    }
}