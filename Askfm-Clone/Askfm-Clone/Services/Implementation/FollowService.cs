using Askfm_Clone.Data;
using Askfm_Clone.DTOs;
using Askfm_Clone.DTOs.Users;
using Askfm_Clone.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Askfm_Clone.Services.Implementation
{
    public class FollowService : IFollowService
    {
        private readonly AppDbContext _appDbContext;

        public FollowService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<bool> FollowAsync(int followerId, int followeeId)
        {
            if (followerId == followeeId) return false;

            var followeeExists = await _appDbContext.Users.AnyAsync(u => u.Id == followeeId);
            if (!followeeExists) return false;

            var alreadyFollowing = await _appDbContext.Follows
                .AnyAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId);

            if (alreadyFollowing) return true;

            var follow = new Follow
            {
                FollowerId = followerId,
                FolloweeId = followeeId,
                CreatedAt = DateTime.UtcNow
            };

            await _appDbContext.Follows.AddAsync(follow);
            await _appDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnfollowAsync(int followerId, int followeeId)
        {
            var follow = await _appDbContext.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId);

            if (follow == null) return false;

            _appDbContext.Follows.Remove(follow);
            await _appDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsFollowingAsync(int followerId, int followeeId)
        {
            return await _appDbContext.Follows
                .AsNoTracking()
                .AnyAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId);
        }

        public async Task<PaginatedResponseDto<UserSummaryDto>> GetFollowersAsync(int userId, int pageNumber, int pageSize)
        {
            var query = _appDbContext.Follows
                .AsNoTracking()
                .Where(f => f.FolloweeId == userId);

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderByDescending(f => f.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new UserSummaryDto
                {
                    Id = f.Follower.Id,
                    Name = f.Follower.Name,
                    ProfilePictureUrl = f.Follower.ProfilePictureUrl
                })
                .ToListAsync();

            return new PaginatedResponseDto<UserSummaryDto>
            {
                Items = items,
                TotalItems = totalItems,
                Page = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PaginatedResponseDto<UserSummaryDto>> GetFollowingAsync(int userId, int pageNumber, int pageSize)
        {
            var query = _appDbContext.Follows
                .AsNoTracking()
                .Where(f => f.FollowerId == userId);

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderByDescending(f => f.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new UserSummaryDto
                {
                    Id = f.Followee.Id,
                    Name = f.Followee.Name,
                    ProfilePictureUrl = f.Followee.ProfilePictureUrl
                })
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
