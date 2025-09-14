using Askfm_Clone.Data;
using Askfm_Clone.DTOs;
using Askfm_Clone.DTOs.Questions;
using Askfm_Clone.Services.Contracts;
using Azure;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Askfm_Clone.Services.Implementation
{
    public class QuestionService : IQuestionService
    {
        private AppDbContext _appDbContext;
        public QuestionService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<int?> CreateQuestion(Question question, int targetUserId)
        {
            var receptor = await _appDbContext.Users.FindAsync(targetUserId);
            if (receptor == null)
            {
                // Or throw a custom NotFoundException
                return null;
            }

            if (question.SenderId.HasValue)
            {
                var senderId = question.SenderId.Value;
                var isBlocked = await _appDbContext.Blocks.AnyAsync(b =>
                (b.BlockerId == senderId && b.BlockedId == receptor.Id) ||
                (b.BlockerId == receptor.Id && b.BlockedId == senderId));
                if (isBlocked)
                    return null;
            }
            if (question.IsAnonymous && !receptor.AllowAnonymous)
            {
                return null;
            }


            // Add the core question object first.
            await _appDbContext.Questions.AddAsync(question);

            // Create the link to the specific recipient.
            var mapping = new QuestionRecipient
            {
                Receptor = receptor,
                Question = question
            };
            await _appDbContext.QuestionRecipients.AddAsync(mapping);

            await _appDbContext.SaveChangesAsync();
            return question.Id;
        }

        public async Task<int?> CreateRandomQuestion(Question question, int numberOfRecipients)
        {
            if (question == null)
                return null;

            if (numberOfRecipients <= 0)
                return null;

            await _appDbContext.Questions.AddAsync(question);

            var baseQuery = _appDbContext.Users.AsNoTracking();
            if (question.SenderId.HasValue)
            {
                var senderId = question.SenderId.Value;
                baseQuery = baseQuery
                                    .Where(u => u.Id != senderId)
                                    .Where(u => !_appDbContext.Blocks.Any(b =>
                (b.BlockerId == senderId && b.BlockedId == u.Id) ||
                (b.BlockerId == u.Id && b.BlockedId == senderId)));
            }
            if (question.IsAnonymous)
            {
                baseQuery = baseQuery.Where(u => u.AllowAnonymous);
            }

            var totalUserCount = await baseQuery.CountAsync();
            List<AppUser> targetUsers;
            if (totalUserCount <= numberOfRecipients)
            {
                targetUsers = await baseQuery.ToListAsync();
            }
            else
            {
                // Note: ORDER BY NEWID() is O(N). Acceptable for small samples; consider alternatives if this becomes hot.
                targetUsers = await baseQuery
                                    .OrderBy(u => Guid.NewGuid())
                                    .Take(numberOfRecipients)
                                    .ToListAsync();
            }

            var mappings = targetUsers.Select(user => new QuestionRecipient
            {
                Question = question,
                Receptor = user
            });

            await _appDbContext.QuestionRecipients.AddRangeAsync(mappings);
            await _appDbContext.SaveChangesAsync();

            return question.Id;
        }

        public async Task<bool> DeleteQuestion(int questionId)
        {
            var question = await _appDbContext.Questions.FindAsync(questionId);
            if (question == null)
                return false;

            _appDbContext.Questions.Remove(question);
            await _appDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<Question?> GetQuestionById(int questionId)
        {
            return await _appDbContext.Questions
                                      .Include(q => q.Sender)
                                      .FirstOrDefaultAsync(q => q.Id == questionId);
        }

        public async Task<PaginatedResponseDto<Question>> GetQuestions(int pageNumber = 1, int pageSize = 10, Expression<Func<Question, bool>> predicate = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 1000) pageSize = 100;

            IQueryable<Question> query = _appDbContext.Questions.AsNoTracking();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            var totalItems = await query.CountAsync();

            var paginatedItems = await query.OrderByDescending(q => q.CreatedAt)
                                            .Skip((pageNumber - 1) * pageSize)
                                            .Take(pageSize)
                                            .ToListAsync();
            return new PaginatedResponseDto<Question>
            {
                Items = paginatedItems,
                TotalItems = totalItems,
                Page = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PaginatedResponseDto<QuestionRecipientDto>> GetReceivedQuestionsAsync(int userId, bool hasAnswered, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 1000) pageSize = 100;

            var query = _appDbContext.QuestionRecipients.AsNoTracking()
                                     .Where(qr => qr.ReceptorId == userId && (hasAnswered ? qr.Answer != null : qr.Answer == null));

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(qr => qr.Question.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(qr => new QuestionRecipientDto
                {
                    QuestionId = qr.QuestionId,
                    Content = qr.Question.Content,
                    SenderId = qr.Question.IsAnonymous ? null : qr.Question.SenderId,
                    IsAnonymous = qr.Question.IsAnonymous,
                    CreatedAt = qr.Question.CreatedAt
                })
                .ToListAsync();

            return new PaginatedResponseDto<QuestionRecipientDto>
            {
                Items = items,
                TotalItems = totalItems,
                Page = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PaginatedResponseDto<QuestionRecipientDto>> GetQuestionsAsync(bool hasAnswered, int pageNumber, int pageSize)
        {
            var query = _appDbContext.QuestionRecipients.AsNoTracking()
                                     .Where(qr => (hasAnswered ? qr.Answer != null : qr.Answer == null));

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(qr => qr.Question.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(qr => new QuestionRecipientDto
                {
                    QuestionId = qr.QuestionId,
                    Content = qr.Question.Content,
                    SenderId = qr.Question.IsAnonymous ? null : qr.Question.SenderId,
                    IsAnonymous = qr.Question.IsAnonymous,
                    CreatedAt = qr.Question.CreatedAt
                })
                .ToListAsync();

            return new PaginatedResponseDto<QuestionRecipientDto>
            {
                Items = items,
                TotalItems = totalItems,
                Page = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
