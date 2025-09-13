using Askfm_Clone.Data;
using Askfm_Clone.DTOs;
using Askfm_Clone.DTOs.Answers;
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
    public class AnswersController : ControllerBase
    {
        private readonly IAnswerService _answerService;

        public AnswersController(IAnswerService answerService)
        {
            _answerService = answerService;
        }

        [HttpGet("{userId:int}/recent")]
        public async Task<ActionResult<PaginatedResponseDto<AnswerDetailsDto>>> GetUserRecentAnswers(
            int userId, [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1, [FromQuery, Range(1, 100)] int pageSize = 10)
        {
            var result = await _answerService.GetPaginatedAnswers(pageNumber, pageSize, userId, OrderAnswersChoice.Recent);
            return Ok(result);
        }

        [HttpGet("{userId:int}/popular")]
        public async Task<ActionResult<PaginatedResponseDto<AnswerDetailsDto>>> GetUserPopularAnswers(
            int userId, [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1, [FromQuery, Range(1, 100)] int pageSize = 10)
        {
            var result = await _answerService.GetPaginatedAnswers(pageNumber, pageSize, userId, OrderAnswersChoice.Popular);
            return Ok(result);
        }

        [HttpGet("me/recent")]
        [Authorize]
        public async Task<ActionResult<PaginatedResponseDto<AnswerDetailsDto>>> GetMyRecentAnswers(
            [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1, [FromQuery, Range(1, 100)] int pageSize = 10)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user authentication");
            }
            var result = await _answerService.GetPaginatedAnswers(pageNumber, pageSize, userId, OrderAnswersChoice.Recent);
            return Ok(result);
        }

        [HttpGet("me/popular")]
        [Authorize]
        public async Task<ActionResult<PaginatedResponseDto<AnswerDetailsDto>>> GetMyPopularAnswers(
           [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1, [FromQuery, Range(1, 100)] int pageSize = 10)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user authentication");
            }
            var result = await _answerService.GetPaginatedAnswers(pageNumber, pageSize, userId, OrderAnswersChoice.Popular);
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Answer>> PostAnswer(PostAnswerDto postAnswerDto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user authentication");
            }

            var answer = new Answer
            {
                Content = postAnswerDto.Content,
                CreatorId = userId
            };

            var newAnswerId = await _answerService.AddAnswer(answer, postAnswerDto.QuestionId, userId);

            if (newAnswerId == null)
            {
                return BadRequest("Question not found, does not belong to you, or has already been answered.");
            }

            answer.Id = newAnswerId.Value;
            return CreatedAtAction(nameof(GetUserRecentAnswers), new { userId = answer.CreatorId }, answer);
        }


        [HttpDelete("{answerId:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteAnswer(int answerId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user authentication");
            }

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
