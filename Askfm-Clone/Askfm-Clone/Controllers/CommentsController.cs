using Askfm_Clone.Data;
using Askfm_Clone.DTOs;
using Askfm_Clone.DTOs.Comments;
using Askfm_Clone.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Askfm_Clone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        /// <summary>
        /// Initializes a new <see cref="CommentsController"/> with its required dependencies.
        /// </summary>
        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        /// <summary>
        /// Returns a paginated list of comments for a specific answer.
        /// </summary>
        /// <param name="answerId">The ID of the answer whose comments are requested.</param>
        /// <param name="page">The page number to retrieve (1-based).</param>
        /// <param name="pageSize">The number of comments per page.</param>
        /// <returns>An <see cref="ActionResult{T}"/> containing a <see cref="PaginatedResponseDto{Comment}"/> with the requested page of comments (HTTP 200).</returns>
        [HttpGet("{answerId}")]
        public async Task<ActionResult<PaginatedResponseDto<Comment>>> GetCommentsForAnswer(
            int answerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _commentService.GetPaginatedComments(page, pageSize, answerId);
            return Ok(result);
        }

        /// <summary>
        /// Creates a new comment for the specified answer using the currently authenticated user.
        /// </summary>
        /// <param name="postCommentDto">DTO containing the comment content and the target AnswerId.</param>
        /// <returns>
        /// 201 Created with the created Comment when successful; 
        /// 400 Bad Request if the target answer does not exist.
        /// </returns>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Comment>> PostComment(PostCommentDto postCommentDto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var comment = new Comment
            {
                Content = postCommentDto.Content,
                CreatedAt = DateTime.UtcNow,
                CreatorId = userId,
                AnswerId = postCommentDto.AnswerId
            };

            var newCommentId = await _commentService.AddComment(comment, userId, postCommentDto.AnswerId);

            if (newCommentId == null)
            {
                return BadRequest("The answer you are trying to comment on does not exist.");
            }

            return CreatedAtAction(nameof(GetCommentsForAnswer), new { answerId = comment.AnswerId }, comment);
        }

        /// <summary>
        /// Deletes the comment with the specified ID.
        /// </summary>
        /// <param name="commentId">The ID of the comment to delete.</param>
        /// <returns>
        /// 204 No Content when the comment was successfully deleted;
        /// 404 Not Found if the comment does not exist;
        /// 403 Forbidden if the caller is not the comment owner and is not in the Admin role.
        /// </returns>
        [HttpDelete("{commentId}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var isOwner = await _commentService.OwnComment(commentId, userId);

            if (!isOwner)
            {
                if (User.IsInRole("Admin"))
                {
                    var adminDeleteResult = await _commentService.DeleteComment(commentId);
                    return adminDeleteResult ? NoContent() : NotFound();
                }
                return Forbid();
            }

            var result = await _commentService.DeleteComment(commentId);
            return result ? NoContent() : NotFound();
        }
    }
}
