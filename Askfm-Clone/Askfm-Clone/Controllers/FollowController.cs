using Askfm_Clone.DTOs;
using Askfm_Clone.DTOs.Users;
using Askfm_Clone.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Askfm_Clone.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class FollowController : BaseController
    {
        private readonly IFollowService _followService;

        public FollowController(IFollowService followService)
        {
            _followService = followService;
        }

        [HttpPost("{targetUserId:int}/follow")]
        [Authorize]
        public async Task<IActionResult> FollowUser(int targetUserId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized("Invalid user authentication");
            }

            var result = await _followService.FollowAsync(userId.Value, targetUserId);
            return result ? NoContent() : BadRequest("Unable to follow the user.");
        }

        [HttpDelete("{targetUserId:int}/unfollow")]
        [Authorize]
        public async Task<IActionResult> UnfollowUser(int targetUserId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized("Invalid user authentication");
            }

            var result = await _followService.UnfollowAsync(userId.Value, targetUserId);
            return result ? NoContent() : NotFound("You are not following this user.");
        }

        [HttpGet("{userId:int}/followers")]
        public async Task<ActionResult<PaginatedResponseDto<UserSummaryDto>>> GetFollowers(
            int userId, [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1, [FromQuery, Range(1, 100)] int pageSize = 10)
        {
            var result = await _followService.GetFollowersAsync(userId, pageNumber, pageSize);
            return Ok(result);
        }

        [HttpGet("{userId:int}/following")]
        public async Task<ActionResult<PaginatedResponseDto<UserSummaryDto>>> GetFollowing(
            int userId, [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1, [FromQuery, Range(1, 100)] int pageSize = 10)
        {
            var result = await _followService.GetFollowingAsync(userId, pageNumber, pageSize);
            return Ok(result);
        }
    }
}
