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

        /// <summary>
        /// Initializes a new instance of <see cref="AnswerService"/> using the provided <see cref="AppDbContext"/>.
        /// </summary>
        public AnswerService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        /// <summary>
        /// Creates and persists a new Answer for the given question recipient if the recipient exists, is unanswered, and the user exists.
        /// </summary>
        /// <param name="answer">The Answer entity to add. Its Creator and QuestionRecipient will be set by this method.</param>
        /// <param name="questionId">The Id of the Question whose recipient should receive the answer.</param>
        /// <param name="userId">The Id of the user creating the answer; used to locate the QuestionRecipient and the Creator user.</param>
        /// <returns>
        /// The Id of the newly created answer, or <c>null</c> if the QuestionRecipient does not exist, is already answered, or the user is not found.
        /// </returns>
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

        /// <summary>
        /// Deletes the answer with the given ID from the database.
        /// </summary>
        /// <param name="answerId">The primary key of the answer to remove.</param>
        /// <returns>True if the answer existed and was removed; false if no answer with the specified ID was found.</returns>
        public async Task<bool> DeleteAnswer(int answerId)
        {
            var answer = await _appDbContext.Answers.FindAsync(answerId);
            if (answer == null)
                return false;

            _appDbContext.Answers.Remove(answer);
            await _appDbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Retrieves a paginated list of answers created by a specific user, projected into AnswerDetailsDto.
        /// </summary>
        /// <remarks>
        /// Results can be ordered by creation time or popularity. The method returns a page of DTOs that include the answer, its question recipient, and whether it has comments; total item count is included for pagination metadata.
        /// </remarks>
        /// <param name="pageNumber">1-based page number to return.</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <param name="userId">Id of the answer creator whose answers should be returned.</param>
        /// <param name="order">Ordering choice: by popularity (likes) or by creation time.</param>
        /// <returns>A PaginatedResponseDto containing the requested page of AnswerDetailsDto items, the total number of matching answers, the current page, and the page size.</returns>
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

        /// <summary>
        /// Determines whether the specified answer was created by the given user.
        /// </summary>
        /// <param name="answerId">The Id of the answer to check.</param>
        /// <param name="userId">The Id of the user to verify as the answer's creator.</param>
        /// <returns>True if an answer with <paramref name="answerId"/> exists and its CreatorId equals <paramref name="userId"/>; otherwise false.</returns>
        public async Task<bool> OwnAnswer(int answerId, int userId)
        {
            return await _appDbContext.Answers.AnyAsync(a => a.Id == answerId && a.CreatorId == userId);
        }
    }
}
