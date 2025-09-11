using Askfm_Clone.Data;
using Askfm_Clone.DTOs;
using Askfm_Clone.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Askfm_Clone.Services.Implementation
{
    public class CommentService : ICommentService
    {
        private AppDbContext _appDbContext;
        /// <summary>
        /// Creates a new instance of <see cref="CommentService"/> and stores the provided <see cref="AppDbContext"/> for use by the service's data operations.
        /// </summary>
        public CommentService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        /// <summary>
        /// Creates and persists a new Comment associated with the specified user and answer.
        /// </summary>
        /// <param name="comment">The Comment entity to add. Its CreatorId/AnswerId will be established by association with the provided user and answer.</param>
        /// <param name="userId">Id of the user creating the comment; must exist in the database.</param>
        /// <param name="answerId">Id of the answer being commented on; must exist in the database.</param>
        /// <returns>
        /// The newly created comment's Id, or null if the specified answer or user does not exist.
        /// </returns>
        public async Task<int?> AddComment(Comment comment, int userId, int answerId)
        {
            // First, ensure the answer the user is trying to comment on actually exists.
            var answer = await _appDbContext.Answers.FirstOrDefaultAsync(ans => ans.Id == answerId);
            if (answer == null)
                return null; // Return null if the answer doesn't exist.

            // Ensure the user who is commenting exists.
            var user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return null; // Return null if the user doesn't exist.

            // EF Core's change tracker is smart. We only need to build the relationships
            // and add the new Comment object. EF Core handles the rest.
            answer.Comments.Add(comment);
            user.Comments.Add(comment);

            await _appDbContext.Comments.AddAsync(comment);
            await _appDbContext.SaveChangesAsync();

            // Return the ID of the newly created comment.
            return comment.Id;
        }

        /// <summary>
        /// Deletes the comment with the specified identifier.
        /// </summary>
        /// <param name="commentId">Identifier of the comment to delete.</param>
        /// <returns>True if the comment was found and removed; false if no comment with the given id exists.</returns>
        public async Task<bool> DeleteComment(int commentId)
        {
            var comment = await _appDbContext.Comments.FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null)
                return false;

            _appDbContext.Comments.Remove(comment);
            await _appDbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Retrieves a paginated list of comments for a specific answer.
        /// </summary>
        /// <param name="pageNumber">1-based page index to return.</param>
        /// <param name="pageSize">Number of comments per page.</param>
        /// <param name="answerId">Identifier of the answer whose comments are requested.</param>
        /// <returns>
        /// A <see cref="PaginatedResponseDto{Comment}"/> containing the requested page of comments, the total comment count for the answer, the returned page number, and the page size. The returned comments are ordered by <c>CreatedAt</c> (ascending) and include each comment's <c>Creator</c> navigation property.
        /// </returns>
        public async Task<PaginatedResponseDto<Comment>> GetPaginatedComments(int pageNumber, int pageSize, int answerId)
        {
            // Start with a base query filtering by the answer.
            IQueryable<Comment> query = _appDbContext.Comments
                                                     .Where(c => c.AnswerId == answerId);

            // Get the total count for pagination metadata *before* applying skip/take.
            var totalItems = await query.CountAsync();

            // Apply ordering, pagination, and includes to fetch the correct page of data.
            var paginatedComments = await query.OrderBy(a => a.CreatedAt)
                                               .Skip((pageNumber - 1) * pageSize)
                                               .Take(pageSize)
                                               .Include(c => c.Creator) // Correctly include the Creator navigation property
                                               .ToListAsync();

            // Create and return the final paginated response object.
            return new PaginatedResponseDto<Comment>
            {
                Items = paginatedComments,
                TotalItems = totalItems,
                Page = pageNumber,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Determines whether the specified user is the creator (owner) of the comment with the given Id.
        /// </summary>
        /// <param name="commentId">The Id of the comment to check.</param>
        /// <param name="userId">The Id of the user to verify as the comment's creator.</param>
        /// <returns>True if the comment exists and its CreatorId matches <paramref name="userId"/>; otherwise false.</returns>
        public async Task<bool> OwnComment(int commentId, int userId)
        {
            var comment = await _appDbContext.Comments.FirstOrDefaultAsync(c => c.Id == commentId);
            // Using a condensed boolean expression for the check.
            return !(comment == null || comment.CreatorId != userId);
        }
    }
}
