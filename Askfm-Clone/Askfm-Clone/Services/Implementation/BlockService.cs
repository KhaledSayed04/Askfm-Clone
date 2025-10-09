using Askfm_Clone.Data;
using Askfm_Clone.DTOs;
using Askfm_Clone.DTOs.Users;
using Askfm_Clone.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Askfm_Clone.Services.Implementation
{
    public class BlockService : IBlockService
    {
        private readonly AppDbContext _appDbContext;

        public BlockService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<bool> BlockAsync(int blockerId, int blockedId, bool isAnonymous = false)
        {
            

            var blockerExists = await _appDbContext.Users.AnyAsync(u => u.Id == blockerId);
            var blockedExists = await _appDbContext.Users.AnyAsync(u => u.Id == blockedId);
            if (!blockerExists || !blockedExists) return false;

            var existingBlock = await _appDbContext.Blocks
                .AnyAsync(b => b.BlockerId == blockerId && b.BlockedId == blockedId);
            if (existingBlock) return true;

            using var transaction = await _appDbContext.Database.BeginTransactionAsync();
            try
            {
                await _appDbContext.Blocks.AddAsync(new Block
                {
                    BlockerId = blockerId,
                    BlockedId = blockedId,
                    IsAnonymous = isAnonymous,
                    CreatedAt = DateTime.UtcNow
                });

                // Remove likes in both directions
                var likesToRemove = await _appDbContext.Likes
                    .Where(l => (l.UserId == blockedId && l.Answer.CreatorId == blockerId) ||
                                (l.UserId == blockerId && l.Answer.CreatorId == blockedId))
                    .ToListAsync();
                _appDbContext.Likes.RemoveRange(likesToRemove);

                // Remove comments in both directions
                var commentsToRemove = await _appDbContext.Comments
                    .Where(c => (c.CreatorId == blockedId && c.Answer.CreatorId == blockerId) ||
                                (c.CreatorId == blockerId && c.Answer.CreatorId == blockedId))
                    .ToListAsync();
                _appDbContext.Comments.RemoveRange(commentsToRemove);

                // Remove follows in both directions
                var followsToRemove = await _appDbContext.Follows
                    .Where(f => (f.FollowerId == blockerId && f.FolloweeId == blockedId) ||
                                (f.FollowerId == blockedId && f.FolloweeId == blockerId))
                    .ToListAsync();
                _appDbContext.Follows.RemoveRange(followsToRemove);

                //  Remove pending question recipients in both directions
                var questionRecipientsToRemove = await _appDbContext.QuestionRecipients
                    .Where(qr => (qr.Question.SenderId == blockerId && qr.ReceptorId == blockedId) ||
                                 (qr.Question.SenderId == blockedId && qr.ReceptorId == blockerId))
                    .ToListAsync();
                _appDbContext.QuestionRecipients.RemoveRange(questionRecipientsToRemove);

                await _appDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                return false;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
        public async Task<bool> UnblockAsync(int blockerId, int blockedId)
        {
            var block = await _appDbContext.Blocks
                .FirstOrDefaultAsync(b => b.BlockerId == blockerId && b.BlockedId == blockedId);

            if (block == null)
            {
                return false;  // there is no block found to remove
            }

            _appDbContext.Blocks.Remove(block);
            await _appDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsBlockedAsync(int userId, int targetUserId)
        {
            return await _appDbContext.Blocks
                .AsNoTracking()
                .AnyAsync(b =>
                    (b.BlockerId == userId && b.BlockedId == targetUserId) ||
                    (b.BlockerId == targetUserId && b.BlockedId == userId));
        }

        public async Task<PaginatedResponseDto<UserSummaryDto>> GetBlockedUsersAsync(int blockerId, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _appDbContext.Blocks
                .AsNoTracking()
                .Where(b => b.BlockerId == blockerId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new UserSummaryDto
                {
                    Id = b.BlockedId,
                    Name = b.Blocked.Name,

                });

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResponseDto<UserSummaryDto>
            {
                Items = items,
                TotalItems = totalItems,
                Page = pageNumber,
                PageSize = pageSize
            };
        }

    }
}