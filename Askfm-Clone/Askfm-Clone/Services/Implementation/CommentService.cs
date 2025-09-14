using Askfm_Clone.Data;
using Askfm_Clone.DTOs;
using Askfm_Clone.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Askfm_Clone.Services.Implementation
{
    public class CommentService : ICommentService
    {
        private AppDbContext _appDbContext;
        public CommentService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public async Task<int?> AddComment(Comment comment, int userId, int answerId)
        {
            var answerExists = await _appDbContext.Answers.AsNoTracking().AnyAsync(ans => ans.Id == answerId);
            if (!answerExists) 
                return null;
            
            var userExists = await _appDbContext.Users.AsNoTracking().AnyAsync(u => u.Id == userId);
            if (!userExists) 
                return null;

            // Set FKs explicitly
            comment.AnswerId = answerId;
            comment.CreatorId = userId;
            // Ensure no nav graphs are tracked accidentally
            comment.Answer = null;
            comment.Creator = null;

            await _appDbContext.Comments.AddAsync(comment);

            try
            {
                await _appDbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Likely FK race (answer/user deleted). Surface as null to the caller.
                return null;
            }

            // Return the ID of the newly created comment.
            return comment.Id;
        }

        public async Task<bool> DeleteComment(int commentId)
        {
            var comment = await _appDbContext.Comments.FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null)
                return false;

            _appDbContext.Comments.Remove(comment);
            await _appDbContext.SaveChangesAsync();
            return true;
        }

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

        public async Task<bool> OwnComment(int commentId, int userId)
        {
            var comment = await _appDbContext.Comments.FirstOrDefaultAsync(c => c.Id == commentId);
            // Using a condensed boolean expression for the check.
            return !(comment == null || comment.CreatorId != userId);
        }
    }
}
