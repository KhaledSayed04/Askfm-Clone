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
        public QuestionsController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        [HttpGet("received")]
        [Authorize]
        public async Task<ActionResult<PaginatedResponseDto<QuestionRecipientDto>>> GetMyReceivedQuestions(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] bool answered = false)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _questionService.GetReceivedQuestionsAsync(userId, answered, page, pageSize);
            return Ok(result);
        }

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

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var result = await _questionService.DeleteQuestion(id);
            return result ? NoContent() : NotFound();
        }
    }
}
