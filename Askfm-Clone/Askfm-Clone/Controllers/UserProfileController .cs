using Askfm_Clone.DTOs.Users;
using Askfm_Clone.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Askfm_Clone.Controllers
{
    public class UserProfileController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;

        public UserProfileController(IUserProfileService userProfileService)
        {
            _userProfileService = userProfileService;
        }

        [HttpGet("{userId:int}")]
        public async Task<ActionResult<UserProfileDto>> GetProfile(int userId)
        {
            var viewerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(viewerIdClaim, out var viewerId); // Allow 0 if not logged in

            var profile = await _userProfileService.GetUserProfileAsync(userId, viewerId);
            if (profile == null) return NotFound("User not found.");

            return Ok(profile);
        }

        [HttpPatch("me/privacy")]
        [Authorize]
        public async Task<IActionResult> UpdatePrivacySettings([FromBody] UpdatePrivacySettingsDto settings)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

            var result = await _userProfileService.UpdatePrivacySettingsAsync(userId, settings.AllowAnonymousQuestions, settings.AllowAnonymousComments);
            return result ? NoContent() : BadRequest("Failed to update privacy settings.");
        }

        [HttpGet("me/coins")]
        [Authorize]
        public async Task<IActionResult> GetMyCoins()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

            var balance = await _userProfileService.GetCoinsBalanceAsync(userId);
            return Ok(new { Balance = balance });
        }

        [HttpGet("me/coins/history")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CoinsTransactionDto>>> GetCoinHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

            var history = await _userProfileService.GetCoinsHistoryAsync(userId, page, pageSize);
            return Ok(history);
        }
    }
}
