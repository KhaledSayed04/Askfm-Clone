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

        public LikeService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

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
            };

            await _appDbContext.Likes.AddAsync(like);
            await _appDbContext.SaveChangesAsync();

            return true;
        }

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

        public async Task<PaginatedResponseDto<LikerDto>> GetAnswerLikersAsync(int answerId, int pageNumber, int pageSize)
        {
            // First, ensure the answer exists before proceeding.
            var answerExists = await _appDbContext.Answers.AsNoTracking().AnyAsync(a => a.Id == answerId);
            if (!answerExists)
                return null; // Signal to the controller that the resource was not found.

            // Build the query to find likes for the specified answer.
            var query = _appDbContext.Likes.AsNoTracking()
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
