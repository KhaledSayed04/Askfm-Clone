using Askfm_Clone.Data;
using Askfm_Clone.DTOs.Users;
using Askfm_Clone.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Askfm_Clone.Services.Implementation
{
    public class UserProfileService : IUserProfileService
    {
        private readonly AppDbContext _appDbContext;

        public UserProfileService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(int userId, int viewerId)
        {
            var user = await _appDbContext.Users
                .AsNoTracking()
                .Include(u => u.Followers)
                .Include(u => u.Following)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return null;

            return new UserProfileDto
            {
                Id = user.Id,
                Name = user.Name,
                AvatarUrl = user.AvatarUrl,
                FollowersCount = user.Followers.Count,
                FollowingCount = user.Following.Count,
                AllowAnonymousQuestions = user.AllowAnonymousQuestions,
                AllowAnonymousComments = user.AllowAnonymousComments
            };
        }

        public async Task<bool> UpdateProfileAsync(int userId, UpdateProfileDto profile)
        {
            var user = await _appDbContext.Users.FindAsync(userId);
            if (user == null) return false;

            user.Name = profile.Name ?? user.Name;
            await _appDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePrivacySettingsAsync(int userId, bool allowAnonymousQuestions, bool allowAnonymousComments)
        {
            var user = await _appDbContext.Users.FindAsync(userId);
            if (user == null) return false;

            user.AllowAnonymousQuestions = allowAnonymousQuestions;
            user.AllowAnonymousComments = allowAnonymousComments;
            await _appDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAvatarAsync(int userId, string avatarUrl)
        {
            var user = await _appDbContext.Users.FindAsync(userId);
            if (user == null) return false;

            user.AvatarUrl = avatarUrl;
            await _appDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetCoinsBalanceAsync(int userId)
        {
            var user = await _appDbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            return user?.Coins ?? 0;
        }

        public async Task<IEnumerable<CoinsTransactionDto>> GetCoinsHistoryAsync(int userId, int page, int pageSize)
        {
            return await _appDbContext.CoinsTransactions
                .AsNoTracking()
                .Where(ct => ct.ReceiverId == userId)
                .OrderByDescending(ct => ct.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ct => new CoinsTransactionDto
                {
                    Id = ct.Id,
                    Amount = ct.Amount,
                    Type = ct.Type.ToString(),
                    CreatedAt = ct.CreatedAt
                })
                .ToListAsync();
        }
    }
}
