using Askfm_Clone.Data;
using Askfm_Clone.DTOs;
using Askfm_Clone.DTOs.Comments;
using Askfm_Clone.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Askfm_Clone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet("{answerId:int}")]
        public async Task<ActionResult<PaginatedResponseDto<Comment>>> GetCommentsForAnswer(
            int answerId, [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1, [FromQuery, Range(1, 100)] int pageSize = 10)
        {
            var result = await _commentService.GetPaginatedComments(pageNumber, pageSize, answerId);
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Comment>> PostComment(PostCommentDto postCommentDto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user authentication");
            }

            var comment = new Comment
            {
                Content = postCommentDto.Content,
                CreatorId = userId,
                AnswerId = postCommentDto.AnswerId
            };

            var newCommentId = await _commentService.AddComment(comment, userId, postCommentDto.AnswerId);

            if (newCommentId == null)
            {
                return NotFound("Answer not found.");
            }

            return CreatedAtAction(nameof(GetCommentsForAnswer),
                                   new { answerId = comment.AnswerId },
                                   new { id = newCommentId, comment.Content, comment.CreatedAt });
        }

        [HttpDelete("{commentId:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user authentication");
            }

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
