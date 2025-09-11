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

        /// <summary>
        /// Initializes a new instance of LikesController with the required like service.
        /// </summary>
        public LikesController(ILikeService likeService)
        {
            _likeService = likeService;
        }

        /// <summary>
        /// Likes the answer with the specified ID on behalf of the current authenticated user.
        /// </summary>
        /// <param name="answerId">The identifier of the answer to like.</param>
        /// <returns>
        /// 200 OK if the like was recorded; 404 NotFound with a message if the specified answer does not exist.
        /// </returns>
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

        /// <summary>
        /// Removes the current authenticated user's like from the specified answer.
        /// </summary>
        /// <param name="answerId">ID of the answer to unlike.</param>
        /// <returns>204 No Content on success; 404 Not Found if the user has not liked the answer (or the answer does not exist).</returns>
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

        /// <summary>
        /// Retrieves a paginated list of users who liked the specified answer.
        /// </summary>
        /// <param name="answerId">ID of the answer whose likers to retrieve.</param>
        /// <param name="page">Page number to return (1-based). Defaults to 1.</param>
        /// <param name="pageSize">Number of items per page. Defaults to 10.</param>
        /// <returns>
        /// 200 OK with a <see cref="PaginatedResponseDto{LikerDto}"/> containing likers when the answer exists;
        /// 404 NotFound with an error message if the answer does not exist.
        /// </returns>
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
