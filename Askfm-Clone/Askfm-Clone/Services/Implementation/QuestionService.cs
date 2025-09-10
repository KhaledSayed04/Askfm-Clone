using Askfm_Clone.Data;
using Askfm_Clone.DTOs;
using Askfm_Clone.DTOs.Questions;
using Askfm_Clone.Services.Contracts;
using Microsoft.EntityFrameworkCore;
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

        public async Task<Question> CreateQuestion(Question question, int targetUserId)
        {
            var receptor = await _appDbContext.Users.FindAsync(targetUserId);
            if (receptor == null)
            {
                // Or throw a custom NotFoundException
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
            return question;
        }

        public async Task<Question> CreateRandomQuestion(Question question, int numberOfRecipients)
        {
            if (question == null)
                throw new ArgumentNullException(nameof(question));

            if (numberOfRecipients <= 0)
                throw new ArgumentException("Number of recipients must be a positive integer.", nameof(numberOfRecipients));

            await _appDbContext.Questions.AddAsync(question);

            var totalUserCount = await _appDbContext.Users.CountAsync();

            List<AppUser> targetUsers;

            if (totalUserCount <= numberOfRecipients)
            {
                targetUsers = await _appDbContext.Users.ToListAsync();
            }
            else
            {
                // Efficiently select random users directly in the database.
                targetUsers = await _appDbContext.Users
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

            return question;
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
            IQueryable<Question> query = _appDbContext.Questions.AsQueryable();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            var totalItems = await query.CountAsync();

            var paginatedItems = await query.Skip((pageNumber - 1) * pageSize)
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
            var query = _appDbContext.QuestionRecipients
                                     .Where(qr => qr.ReceptorId == userId && qr.AnswerId.HasValue == hasAnswered);

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(qr => qr.Question.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(qr => new QuestionRecipientDto
                {
                    QuestionId = qr.QuestionId,
                    Content = qr.Question.Content,
                    SenderName = qr.Question.IsAnonymous ? null : qr.Question.Sender.Name,
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
