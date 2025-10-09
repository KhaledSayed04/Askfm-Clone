namespace Askfm_Clone.Services.Contracts
{
    public interface IBlockService
    {
        Task<bool> BlockUserAsync(int blockerId, int blockedId, bool isAnonymous = false);
        Task<bool> UnblockUserAsync(int blockerId, int blockedId);
        Task<bool> IsBlockedAsync(int userId, int targetUserId);
        // user’s block list
        Task<IEnumerable<int>> GetBlockedUsersAsync(int blockerId); 

    }
}
