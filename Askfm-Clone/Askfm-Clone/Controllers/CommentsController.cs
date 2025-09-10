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

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet("{answerId}")]
        public async Task<ActionResult<PaginatedResponseDto<Comment>>> GetCommentsForAnswer(
            int answerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _commentService.GetPaginatedComments(page, pageSize, answerId);
            return Ok(result);
        }

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
