using Askfm_Clone.Data;
using Askfm_Clone.DTOs;
using Askfm_Clone.DTOs.Questions;
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
    public class QuestionsController : BaseController
    {
        private readonly IQuestionService _questionService;
        public QuestionsController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        [HttpGet("received")]
        [Authorize]
        public async Task<ActionResult<PaginatedResponseDto<QuestionRecipientDto>>> GetMyReceivedQuestions(
            [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1, [FromQuery, Range(1, 100)] int pageSize = 10, [FromQuery] bool answered = false)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized("Invalid user authentication");
            }
            var result = await _questionService.GetReceivedQuestionsAsync(userId.Value, answered, pageNumber, pageSize);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaginatedResponseDto<QuestionRecipientDto>>> GetQuestions(
            [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1, [FromQuery, Range(1, 100)] int pageSize = 10, [FromQuery] bool answered = false)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized("Invalid user authentication");
            }
            var result = await _questionService.GetQuestionsAsync(answered, pageNumber, pageSize);
            return Ok(result);
        }

        [HttpPost]
        [Authorize] // User must be logged in to ask a question
        public async Task<ActionResult<int>> CreateQuestion(PostQuestionDto questionDto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized("Invalid user authentication");
            }

            var question = new Question
            {
                Content = questionDto.Content,
                IsAnonymous = questionDto.IsAnonymous,
                SenderId = questionDto.IsAnonymous ? null : userId
            };

            var newQuestionId = await _questionService.CreateQuestion(question, questionDto.ToUserId);

            if (newQuestionId == null)
            {
                return NotFound("The user you are trying to send them a question do not exist or blocked you.");
            }

            return Created(string.Empty, newQuestionId);
        }

        [HttpPost("random")]
        [Authorize]
        public async Task<ActionResult<int>> CreateRandomQuestion(PostRandomQuestionDto questionDto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized("Invalid user authentication");
            }

            var question = new Question
            {
                Content = questionDto.Content,
                IsAnonymous = questionDto.IsAnonymous,
                SenderId = questionDto.IsAnonymous ? null : userId
            };

            var newQuestionId = await _questionService.CreateRandomQuestion(question, questionDto.NumberOfRecipients);

            return Created(string.Empty, newQuestionId);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var result = await _questionService.DeleteQuestion(id);
            return result ? NoContent() : NotFound();
        }
    }
}
