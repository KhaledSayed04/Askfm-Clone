using Askfm_Clone.Data;
using Askfm_Clone.DTOs;
using Askfm_Clone.DTOs.Likes;
using Askfm_Clone.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Askfm_Clone.Services.Implementation
{
    public class LikeService : ILikeService
    {
        private readonly AppDbContext _appDbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="LikeService"/> class with the provided application database context.
        /// </summary>
        public LikeService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        /// <summary>
        /// Adds a like from the specified user to the specified answer.
        /// </summary>
        /// <param name="userId">ID of the user who is liking the answer.</param>
        /// <param name="answerId">ID of the answer to like.</param>
        /// <returns>
        /// True if the like now exists (either it was created or had already existed); false if the target answer does not exist.
        /// </returns>
        /// <remarks>
        /// The operation is idempotent: calling it when the user already liked the answer leaves state unchanged and returns true.
        /// On success this persists a new Like entity with CreatedAt set to UTC now.
        /// </remarks>
        public async Task<bool> LikeAnswerAsync(int userId, int answerId)
        {
            // 1. Check if the answer exists.
            var answerExists = await _appDbContext.Answers.AnyAsync(a => a.Id == answerId);
            if (!answerExists)
            {
                return false; // The answer to be liked doesn't exist.
            }

            // 2. Check if the user has already liked this answer to prevent duplicates.
            var alreadyLiked = await _appDbContext.Likes
                .AnyAsync(l => l.UserId == userId && l.AnswerId == answerId);

            if (alreadyLiked)
            {
                return true; // The user has already liked this, so the state is already correct.
            }

            // 3. Create and add the new like.
            var like = new Like
            {
                UserId = userId,
                AnswerId = answerId,
                CreatedAt = DateTime.UtcNow
            };

            await _appDbContext.Likes.AddAsync(like);
            await _appDbContext.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Removes a user's like from an answer if it exists.
        /// </summary>
        /// <param name="userId">ID of the user attempting to unlike the answer.</param>
        /// <param name="answerId">ID of the answer to unlike.</param>
        /// <returns>
        /// True if a like was found and removed; false if no like existed for the given user and answer.
        /// </returns>
        public async Task<bool> UnlikeAnswerAsync(int userId, int answerId)
        {
            // Find the specific like to remove.
            var like = await _appDbContext.Likes
                .FirstOrDefaultAsync(l => l.UserId == userId && l.AnswerId == answerId);

            if (like == null)
            {
                return false; // The user hasn't liked this answer, so there's nothing to remove.
            }

            _appDbContext.Likes.Remove(like);
            await _appDbContext.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Removes all likes made by a specific user (the "liker") on answers created by another user (the "blocker").
        /// </summary>
        /// <param name="likerId">ID of the user whose likes will be removed.</param>
        /// <param name="blockerId">ID of the answer creator whose answers were liked by <paramref name="likerId"/>.</param>
        public async Task RemoveAllLikesFromUserAsync(int likerId, int blockerId)
        {
            // This query efficiently finds all likes where the 'liker' is the user being blocked,
            // and the 'answer creator' is the user who is initiating the block.
            var likesToRemove = await _appDbContext.Likes
                .Where(like => like.UserId == likerId && like.Answer.CreatorId == blockerId)
                .ToListAsync();

            if (likesToRemove.Any())
            {
                // RemoveRange is used for bulk deletion, which is more performant than deleting one by one.
                _appDbContext.Likes.RemoveRange(likesToRemove);
                await _appDbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Retrieves a paginated list of users who liked the specified answer.
        /// </summary>
        /// <remarks>
        /// Returns null if the answer does not exist. Results are ordered by like time (most recent first).
        /// </remarks>
        /// <param name="answerId">The ID of the answer whose likers to retrieve.</param>
        /// <param name="pageNumber">The 1-based page number to return.</param>
        /// <param name="pageSize">The maximum number of items to include in a page.</param>
        /// <returns>
        /// A <see cref="PaginatedResponseDto{LikerDto}"/> containing the likers for the requested page, or null if the answer was not found.
        /// </returns>
        public async Task<PaginatedResponseDto<LikerDto>> GetAnswerLikersAsync(int answerId, int pageNumber, int pageSize)
        {
            // First, ensure the answer exists before proceeding.
            var answerExists = await _appDbContext.Answers.AnyAsync(a => a.Id == answerId);
            if (!answerExists)
            {
                return null; // Signal to the controller that the resource was not found.
            }

            // Build the query to find likes for the specified answer.
            var query = _appDbContext.Likes
                .Where(l => l.AnswerId == answerId);

            // Get the total count for the pagination metadata.
            var totalItems = await query.CountAsync();

            // Fetch the paginated list of likers, projecting directly into the DTO.
            // This is a performant query that only selects the required user data.
            var likers = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new LikerDto
                {
                    UserId = l.User.Id,
                    UserName = l.User.Name,
                    LikedAt = l.CreatedAt
                })
                .ToListAsync();

            return new PaginatedResponseDto<LikerDto>
            {
                Items = likers,
                TotalItems = totalItems,
                Page = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
