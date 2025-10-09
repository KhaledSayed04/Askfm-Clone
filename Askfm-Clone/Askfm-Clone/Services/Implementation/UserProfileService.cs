namespace Askfm_Clone.Services.Implementation
{
    public interface IUserProfileService
    {
        Task<UserProfileDto?> GetUserProfileAsync(int userId, int viewerId);
        Task<bool> UpdateProfileAsync(int userId, UpdateUserProfileDto profile);
        Task<bool> UpdatePrivacySettingsAsync(int userId, bool allowAnonymousQuestions, bool allowAnonymousComments);
        Task<bool> UpdateAvatarAsync(int userId, string avatarUrl);

        Task<int> GetCoinsBalanceAsync(int userId);
        Task<IEnumerable<CoinsTransactionDto>> GetCoinsHistoryAsync(int userId, int page, int pageSize);
    }

}
