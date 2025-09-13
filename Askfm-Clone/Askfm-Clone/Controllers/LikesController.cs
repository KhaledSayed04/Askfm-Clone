using Askfm_Clone.DTOs;
using Askfm_Clone.DTOs.Likes;
using Askfm_Clone.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Askfm_Clone.Controllers
{
    [Route("api/likes")]
    [ApiController]
    [Authorize] // All actions in this controller require the user to be logged in.
    public class LikesController : ControllerBase
    {
        private readonly ILikeService _likeService;

        public LikesController(ILikeService likeService)
        {
            _likeService = likeService;
        }

        [HttpPost("{answerId:int}")]
        public async Task<IActionResult> LikeAnswer(int answerId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user authentication");
            }

            var result = await _likeService.LikeAnswerAsync(userId, answerId);

            return result ? NoContent() : NotFound("The answer you are trying to like does not exist.");
        }

        [HttpDelete("{answerId:int}")]
        public async Task<IActionResult> UnlikeAnswer(int answerId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user authentication");
            }

            var result = await _likeService.UnlikeAnswerAsync(userId, answerId);

            return result ? NoContent() : NotFound("You have not liked this answer, so you cannot unlike it."); // 204 No Content is standard for a successful DELETE.
        }

        [HttpGet("{answerId:int}/users")]
        [AllowAnonymous] // It's common for likers to be publicly visible.
        public async Task<ActionResult<PaginatedResponseDto<LikerDto>>> GetLikersForAnswer(
            int answerId, [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1, [FromQuery, Range(1, 100)] int pageSize = 10)
        {
            var result = await _likeService.GetAnswerLikersAsync(answerId, pageNumber, pageSize);

            if (result == null)
            {
                return NotFound("The specified answer does not exist.");
            }

            return Ok(result);
        }
    }
}
