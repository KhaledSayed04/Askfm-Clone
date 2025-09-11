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
        /// <summary>
        /// Initializes a new instance of <see cref="QuestionService"/> with the provided application database context.
        /// </summary>
        public QuestionService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        /// <summary>
        /// Persists a new Question and links it to a single recipient user by creating a QuestionRecipient mapping.
        /// </summary>
        /// <param name="question">The Question entity to create (must be non-null and ready to be saved).</param>
        /// <param name="targetUserId">ID of the recipient user who will receive the question.</param>
        /// <returns>
        /// The saved Question including any database-generated values, or null if no user exists with <paramref name="targetUserId"/>.
        /// </returns>
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

        /// <summary>
        /// Persists a new question and assigns it to a set of recipients chosen at random.
        /// </summary>
        /// <remarks>
        /// If the total number of users is less than or equal to <paramref name="numberOfRecipients"/>, the question is assigned to all users; otherwise a random subset of users is selected.
        /// </remarks>
        /// <param name="question">The question to create. Must not be null.</param>
        /// <param name="numberOfRecipients">Number of recipients to assign the question to; must be a positive integer.</param>
        /// <returns>The saved <see cref="Question"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="question"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="numberOfRecipients"/> is less than or equal to zero.</exception>
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

        /// <summary>
        /// Deletes the question with the given ID from the database.
        /// </summary>
        /// <param name="questionId">The primary key of the question to delete.</param>
        /// <returns>
        /// True if the question was found and deleted; false if no question with the specified ID exists.
        /// </returns>
        public async Task<bool> DeleteQuestion(int questionId)
        {
            var question = await _appDbContext.Questions.FindAsync(questionId);
            if (question == null)
                return false;

            _appDbContext.Questions.Remove(question);
            await _appDbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Retrieves a question by its identifier, including the question's Sender.
        /// </summary>
        /// <param name="questionId">The primary key of the question to retrieve.</param>
        /// <returns>The matching <see cref="Question"/> including its Sender, or <c>null</c> if no question with the given id exists.</returns>
        public async Task<Question?> GetQuestionById(int questionId)
        {
            return await _appDbContext.Questions
                                      .Include(q => q.Sender)
                                      .FirstOrDefaultAsync(q => q.Id == questionId);
        }

        /// <summary>
        /// Retrieves a paginated list of questions, optionally filtered by the provided predicate.
        /// </summary>
        /// <param name="pageNumber">1-based page index to return. Defaults to 1.</param>
        /// <param name="pageSize">Number of items per page. Defaults to 10.</param>
        /// <param name="predicate">Optional filter expression applied to questions before pagination.</param>
        /// <returns>
        /// A <see cref="PaginatedResponseDto{Question}"/> containing the page of questions, the total item count, the requested page number, and the page size.
        /// </returns>
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

        /// <summary>
        /// Retrieves a paginated list of questions received by a user, optionally filtered by whether they've been answered.
        /// </summary>
        /// <param name="userId">The recipient user's ID whose received questions to fetch.</param>
        /// <param name="hasAnswered">If true, returns only questions that have an associated answer; if false, only unanswered questions.</param>
        /// <param name="pageNumber">1-based page number to return.</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <returns>
        /// A <see cref="PaginatedResponseDto{QuestionRecipientDto}"/> containing the requested page of received questions.
        /// Each item includes QuestionId, Content, SenderName (null when the question was sent anonymously), IsAnonymous and CreatedAt.
        /// </returns>
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
