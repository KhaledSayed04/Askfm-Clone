using Askfm_Clone.DTOs;
using Askfm_Clone.DTOs.Likes;
using Askfm_Clone.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        [HttpPost("{answerId}")]
        public async Task<IActionResult> LikeAnswer(int answerId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var result = await _likeService.LikeAnswerAsync(userId, answerId);

            if (!result)
            {
                return NotFound("The answer you are trying to like does not exist.");
            }

            return Ok();
        }

        [HttpDelete("{answerId}")]
        public async Task<IActionResult> UnlikeAnswer(int answerId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var result = await _likeService.UnlikeAnswerAsync(userId, answerId);

            if (!result)
            {
                return NotFound("You have not liked this answer, so you cannot unlike it.");
            }

            return NoContent(); // 204 No Content is standard for a successful DELETE.
        }

        [HttpGet("{answerId}/users")]
        [AllowAnonymous] // It's common for likers to be publicly visible.
        public async Task<ActionResult<PaginatedResponseDto<LikerDto>>> GetLikersForAnswer(
            int answerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _likeService.GetAnswerLikersAsync(answerId, page, pageSize);

            if (result == null)
            {
                return NotFound("The specified answer does not exist.");
            }

            return Ok(result);
        }
    }
}
