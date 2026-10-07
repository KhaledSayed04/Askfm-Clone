using Askfm_Clone.DTOs.Users;

namespace Askfm_Clone.Services.Contracts
{
    public interface IUserProfileService
    {
        Task<UserProfileDto?> GetUserProfileAsync(int userId, int viewerId);
        Task<bool> UpdateProfileAsync(int userId, UpdateProfileDto profile);
        Task<bool> UpdatePrivacySettingsAsync(int userId, bool allowAnonymous);
        Task<bool> UpdateAvatarAsync(int userId, string profilePictureUrl);

        Task<int> GetCoinsBalanceAsync(int userId);
        Task<IEnumerable<CoinsTransactionDto>> GetCoinsHistoryAsync(int userId, int page, int pageSize);
    }
}
