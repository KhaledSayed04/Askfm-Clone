using Askfm_Clone.DTOs;
using Askfm_Clone.DTOs.Users;

namespace Askfm_Clone.Services.Contracts
{
    public interface IBlockService
    {
        Task<bool> BlockAsync(int blockerId, int blockedId, bool isAnonymous = false);
        Task<bool> UnblockAsync(int blockerId, int blockedId);
        Task<bool> IsBlockedAsync(int userId, int targetUserId);
        Task<PaginatedResponseDto<UserSummaryDto>> GetBlockedUsersAsync(int blockerId, int pageNumber, int pageSize);
    }
}