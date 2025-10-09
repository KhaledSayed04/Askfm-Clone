using Askfm_Clone.DTOs;
using Askfm_Clone.DTOs.Users;

namespace Askfm_Clone.Services.Contracts
{
    public interface IFollowService
    {
        Task<bool> FollowAsync(int followerId, int followeeId);
        Task<bool> UnfollowAsync(int followerId, int followeeId);
        Task<bool> IsFollowingAsync(int followerId, int followeeId);

        Task<PaginatedResponseDto<UserSummaryDto>> GetFollowersAsync(int userId, int pageNumber, int pageSize);
        Task<PaginatedResponseDto<UserSummaryDto>> GetFollowingAsync(int userId, int pageNumber, int pageSize);
    }

}
