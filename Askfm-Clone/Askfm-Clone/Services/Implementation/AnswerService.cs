using Askfm_Clone.Data;
using Askfm_Clone.DTOs;
using Askfm_Clone.DTOs.Answers;
using Askfm_Clone.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Askfm_Clone.Services.Implementation
{
    public class AnswerService : IAnswerService
    {
        private readonly AppDbContext _appDbContext;

        public AnswerService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<int?> AddAnswer(Answer answer, int questionId, int userId)
        {
            var questionRecipient = await _appDbContext.QuestionRecipients.FirstOrDefaultAsync(q => q.QuestionId == questionId && q.ReceptorId == userId);
            if (questionRecipient == null || questionRecipient.Answer != null)
                return null; // Question not found or already answered

            var user = await _appDbContext.Users.FindAsync(userId);
            if (user == null) return null;

            answer.Creator = user;
            answer.QuestionRecipient = questionRecipient;

            await _appDbContext.Answers.AddAsync(answer);
            await _appDbContext.SaveChangesAsync();

            return answer.Id;
        }

        public async Task<bool> DeleteAnswer(int answerId)
        {
            var answer = await _appDbContext.Answers.FindAsync(answerId);
            if (answer == null)
                return false;

            _appDbContext.Answers.Remove(answer);
            await _appDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<PaginatedResponseDto<AnswerDetailsDto>> GetPaginatedAnswers(
            int pageNumber, int pageSize, int userId, OrderAnswersChoice order)
        {
            // Base query for answers created by the specified user.
            IQueryable<Answer> query = _appDbContext.Answers
                                                     .Where(a => a.CreatorId == userId);

            // Apply ordering
            if (order == OrderAnswersChoice.Popular)
            {
                query = query.OrderByDescending(a => a.Likes.Count()).ThenBy(a => a.CreatedAt);
            }
            else
            {
                query = query.OrderByDescending(a => a.CreatedAt);
            }

            var totalItems = await query.CountAsync();

            // *** PERFORMANCE OPTIMIZATION ***
            // Project to the DTO *before* fetching the data.
            // This creates an efficient SQL query that only selects the needed columns.
            var dtoItems = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AnswerDetailsDto
                {
                    Answer = a, // For simplicity; could be mapped to a smaller Answer DTO
                    QuestionRecipient = a.QuestionRecipient,
                    HasComments = a.Comments.Any()
                })
                .ToListAsync();

            return new PaginatedResponseDto<AnswerDetailsDto>
            {
                Items = dtoItems,
                TotalItems = totalItems,
                Page = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<bool> OwnAnswer(int answerId, int userId)
        {
            return await _appDbContext.Answers.AnyAsync(a => a.Id == answerId && a.CreatorId == userId);
        }
    }
}
