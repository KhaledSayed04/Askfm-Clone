using Askfm_Clone.Data;
using Askfm_Clone.DTOs;
using Askfm_Clone.DTOs.Answers;
using Askfm_Clone.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Askfm_Clone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnswersController : ControllerBase
    {
        private readonly IAnswerService _answerService;

        public AnswersController(IAnswerService answerService)
        {
            _answerService = answerService;
        }

        [HttpGet("{userId}/recent")]
        public async Task<ActionResult<PaginatedResponseDto<AnswerDetailsDto>>> GetUserRecentAnswers(
            int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _answerService.GetPaginatedAnswers(page, pageSize, userId, OrderAnswersChoice.Recent);
            return Ok(result);
        }

        [HttpGet("{userId}/popular")]
        public async Task<ActionResult<PaginatedResponseDto<AnswerDetailsDto>>> GetUserPopularAnswers(
            int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _answerService.GetPaginatedAnswers(page, pageSize, userId, OrderAnswersChoice.Popular);
            return Ok(result);
        }

        [HttpGet("me/recent")]
        [Authorize]
        public async Task<ActionResult<PaginatedResponseDto<AnswerDetailsDto>>> GetMyRecentAnswers(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _answerService.GetPaginatedAnswers(page, pageSize, userId, OrderAnswersChoice.Recent);
            return Ok(result);
        }

        [HttpGet("me/popular")]
        [Authorize]
        public async Task<ActionResult<PaginatedResponseDto<AnswerDetailsDto>>> GetMyPopularAnswers(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _answerService.GetPaginatedAnswers(page, pageSize, userId, OrderAnswersChoice.Popular);
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Answer>> PostAnswer(PostAnswerDto postAnswerDto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var answer = new Answer
            {
                Content = postAnswerDto.Content,
                CreatedAt = DateTime.UtcNow,
                CreatorId = userId
            };

            var newAnswerId = await _answerService.AddAnswer(answer, postAnswerDto.QuestionId, userId);

            if (newAnswerId == null)
            {
                return BadRequest("Question not found, does not belong to you, or has already been answered.");
            }

            // It's good practice to return the created object and a link to it.
            return CreatedAtAction(nameof(GetUserRecentAnswers), new { userId = answer.CreatorId }, answer);
        }


        [HttpDelete("{answerId}")]
        [Authorize]
        public async Task<IActionResult> DeleteAnswer(int answerId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Check if the user owns the answer before allowing deletion.
            var isOwner = await _answerService.OwnAnswer(answerId, userId);

            if (!isOwner)
            {
                // Admins can delete any answer.
                if (User.IsInRole("Admin"))
                {
                    var adminDeleteResult = await _answerService.DeleteAnswer(answerId);
                    return adminDeleteResult ? NoContent() : NotFound();
                }
                return Forbid(); // User is not the owner and not an admin.
            }

            var result = await _answerService.DeleteAnswer(answerId);

            return result ? NoContent() : NotFound(); // 204 No Content on successful deletion.
        }
    }
}
