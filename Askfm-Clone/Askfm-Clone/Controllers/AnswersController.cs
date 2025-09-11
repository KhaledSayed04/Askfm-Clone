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

        /// <summary>
        /// Initializes a new instance of the <see cref="AnswersController"/> with the required answer service dependency.
        /// </summary>
        public AnswersController(IAnswerService answerService)
        {
            _answerService = answerService;
        }

        /// <summary>
        /// Returns a paginated list of a user's answers ordered by most recent.
        /// </summary>
        /// <param name="userId">ID of the user whose answers to retrieve.</param>
        /// <param name="page">Page number (1-based). Defaults to 1.</param>
        /// <param name="pageSize">Number of items per page. Defaults to 10.</param>
        /// <returns>200 OK with a <see cref="PaginatedResponseDto{AnswerDetailsDto}"/> containing the requested page of answers ordered by recency.</returns>
        [HttpGet("{userId}/recent")]
        public async Task<ActionResult<PaginatedResponseDto<AnswerDetailsDto>>> GetUserRecentAnswers(
            int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _answerService.GetPaginatedAnswers(page, pageSize, userId, OrderAnswersChoice.Recent);
            return Ok(result);
        }

        /// <summary>
        /// Returns a paginated list of a user's answers ordered by popularity.
        /// </summary>
        /// <param name="userId">ID of the user whose answers to retrieve.</param>
        /// <param name="page">Page number (1-based). Defaults to 1.</param>
        /// <param name="pageSize">Number of items per page. Defaults to 10.</param>
        /// <returns>A paginated response containing answer detail DTOs ordered by popularity.</returns>
        [HttpGet("{userId}/popular")]
        public async Task<ActionResult<PaginatedResponseDto<AnswerDetailsDto>>> GetUserPopularAnswers(
            int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _answerService.GetPaginatedAnswers(page, pageSize, userId, OrderAnswersChoice.Popular);
            return Ok(result);
        }

        /// <summary>
        /// Returns a paginated list of the authenticated user's answers ordered by most recent.
        /// </summary>
        /// <remarks>
        /// Requires an authenticated user; the user id is taken from the current principal's NameIdentifier claim.
        /// </remarks>
        /// <param name="page">Page number (1-based).</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <returns>A <see cref="PaginatedResponseDto{AnswerDetailsDto}"/> containing the requested page of recent answers.</returns>
        [HttpGet("me/recent")]
        [Authorize]
        public async Task<ActionResult<PaginatedResponseDto<AnswerDetailsDto>>> GetMyRecentAnswers(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _answerService.GetPaginatedAnswers(page, pageSize, userId, OrderAnswersChoice.Recent);
            return Ok(result);
        }

        /// <summary>
        /// Returns a paginated list of the authenticated user's answers ordered by popularity.
        /// </summary>
        /// <remarks>
        /// Requires an authenticated user (reads the user id from the NameIdentifier claim).
        /// </remarks>
        /// <param name="page">Page number to return (1-based). Default is 1.</param>
        /// <param name="pageSize">Number of items per page. Default is 10.</param>
        /// <returns>200 OK with a <see cref="PaginatedResponseDto{AnswerDetailsDto}"/> containing the requested page of answers.</returns>
        [HttpGet("me/popular")]
        [Authorize]
        public async Task<ActionResult<PaginatedResponseDto<AnswerDetailsDto>>> GetMyPopularAnswers(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _answerService.GetPaginatedAnswers(page, pageSize, userId, OrderAnswersChoice.Popular);
            return Ok(result);
        }

        /// <summary>
        /// Creates a new answer for the authenticated user.
        /// </summary>
        /// <remarks>
        /// Requires an authenticated user (reads user id from the NameIdentifier claim).
        /// Constructs an <see cref="Answer"/> using the provided <paramref name="postAnswerDto"/>'s content and the current UTC time,
        /// then attempts to add it to the indicated question.
        /// </remarks>
        /// <param name="postAnswerDto">DTO containing the answer content and the target QuestionId.</param>
        /// <returns>
        /// 201 Created with the created <see cref="Answer"/> when successful; 
        /// 400 Bad Request with a message when the question was not found, does not belong to the user, or has already been answered.
        /// </returns>
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


        /// <summary>
        /// Deletes the specified answer if the caller is the answer's owner or an administrator.
        /// </summary>
        /// <remarks>
        /// Requires an authenticated user. Owners may delete their own answers; users in the "Admin" role may delete any answer.
        /// </remarks>
        /// <param name="answerId">ID of the answer to delete.</param>
        /// <returns>
        /// 204 No Content when the answer was deleted;
        /// 404 Not Found if the answer does not exist;
        /// 403 Forbidden when the caller is neither the owner nor an admin.
        /// </returns>
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
