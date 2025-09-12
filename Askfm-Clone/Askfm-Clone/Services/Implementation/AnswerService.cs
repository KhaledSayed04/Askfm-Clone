using Askfm_Clone.Data;
using Askfm_Clone.DTOs;
using Askfm_Clone.DTOs.Answers;
using Askfm_Clone.Services.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;
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
            using var transaction = await _appDbContext.Database.BeginTransactionAsync();
            try
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
                await transaction.CommitAsync();
                return answer.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                return null;
            }
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
                                                     .Include(a => a.QuestionRecipient)
                                                     .ThenInclude(qr => qr.Question)
                                                     .Where(a => a.CreatorId == userId);

            // Apply ordering
            if (order == OrderAnswersChoice.Popular)
            {
                query = query.Select(a => new { Answer = a, LikeCount = a.Likes.Count() })
                             .OrderByDescending(x => x.LikeCount)
                             .ThenBy(x => x.Answer.CreatedAt)
                             .Select(x => x.Answer);
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
                    AnswerId = a.Id, // For simplicity; could be mapped to a smaller Answer DTO
                    QuestionId = a.QuestionId,
                    RecipientId = a.ReceptorId,
                    CreatorId = a.CreatorId,
                    Content = a.Content,
                    CreatedAt = a.CreatedAt,
                    LikesCount = a.Likes.Count(),
                    CommentsCount = a.Comments.Count(),
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
