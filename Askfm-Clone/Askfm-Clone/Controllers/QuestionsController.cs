using Askfm_Clone.Data;
using Askfm_Clone.DTOs;
using Askfm_Clone.DTOs.Questions;
using Askfm_Clone.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Askfm_Clone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {
        private readonly IQuestionService _questionService;
        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionsController"/> with its required dependencies.
        /// </summary>
        public QuestionsController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        /// <summary>
        /// Retrieves a paginated list of questions received by the authenticated user.
        /// </summary>
        /// <remarks>
        /// Requires authentication. The current user's id is taken from the NameIdentifier claim.
        /// </remarks>
        /// <param name="page">Page number to return (1-based). Default is 1.</param>
        /// <param name="pageSize">Number of items per page. Default is 10.</param>
        /// <param name="answered">If true, returns only answered questions; if false, returns only unanswered questions. Default is false.</param>
        /// <returns>200 OK with a PaginatedResponseDto&lt;QuestionRecipientDto&gt; containing the requested page of received questions.</returns>
        [HttpGet("received")]
        [Authorize]
        public async Task<ActionResult<PaginatedResponseDto<QuestionRecipientDto>>> GetMyReceivedQuestions(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] bool answered = false)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _questionService.GetReceivedQuestionsAsync(userId, answered, page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Creates a new question from the currently authenticated user and sends it to the specified recipient.
        /// </summary>
        /// <remarks>
        /// If <paramref name="questionDto"/>.IsAnonymous is true, the question's FromUserId will be null; otherwise it will be set to the caller's user id.
        /// Requires an authenticated user.
        /// </remarks>
        /// <param name="questionDto">Payload containing the question content, target recipient id, and anonymity flag.</param>
        /// <returns>
        /// 201 Created with the created <see cref="Question"/> on success; 400 Bad Request if the target user does not exist.
        /// </returns>
        [HttpPost]
        [Authorize] // User must be logged in to ask a question
        public async Task<ActionResult<Question>> CreateQuestion(PostQuestionDto questionDto)
        {
            var senderId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var question = new Question
            {
                Content = questionDto.Content,
                CreatedAt = DateTime.UtcNow,
                IsAnonymous = questionDto.IsAnonymous,
                FromUserId = questionDto.IsAnonymous ? null : senderId
            };

            var newQuestion = await _questionService.CreateQuestion(question, questionDto.ToUserId);

            if (newQuestion == null)
            {
                return BadRequest("The user you are trying to send a question to does not exist.");
            }

            return CreatedAtAction(nameof(GetMyReceivedQuestions), newQuestion);
        }

        /// <summary>
        /// Creates a question from the authenticated user and delivers it to a specified number of random recipients.
        /// </summary>
        /// <param name="questionDto">Payload with the question content, whether it is anonymous, and the number of recipients.</param>
        /// <returns>200 OK with the created <see cref="Question"/> distributed to random recipients.</returns>
        [HttpPost("random")]
        [Authorize]
        public async Task<ActionResult<Question>> CreateRandomQuestion(PostRandomQuestionDto questionDto)
        {
            var senderId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var question = new Question
            {
                Content = questionDto.Content,
                CreatedAt = DateTime.UtcNow,
                IsAnonymous = questionDto.IsAnonymous,
                FromUserId = questionDto.IsAnonymous ? null : senderId
            };

            var newQuestion = await _questionService.CreateRandomQuestion(question, questionDto.NumberOfRecipients);

            return Ok(newQuestion);
        }

        /// <summary>
        /// Deletes the question with the specified identifier. Requires an authenticated user in the "Admin" role.
        /// </summary>
        /// <param name="id">The identifier of the question to delete.</param>
        /// <returns>
        /// 204 No Content when the question was deleted; 404 Not Found when no question with the given <paramref name="id"/> exists.
        /// </returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var result = await _questionService.DeleteQuestion(id);
            return result ? NoContent() : NotFound();
        }
    }
}
