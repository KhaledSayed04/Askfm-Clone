using Askfm_Clone.DTOs;
using Askfm_Clone.DTOs.Likes;

namespace Askfm_Clone.Services.Contracts
{
    public interface ILikeService
    {
        /// <summary>
        /// Adds a like from a user to a specific answer.
        /// </summary>
        /// <param name="userId">The ID of the user who is liking the answer.</param>
        /// <param name="answerId">The ID of the answer to be liked.</param>
        /// <summary>
/// Adds a like from the specified user to the specified answer.
/// </summary>
/// <param name="userId">ID of the user who is liking the answer.</param>
/// <param name="answerId">ID of the answer to be liked.</param>
/// <returns>
/// True if the like was recorded; false if the target answer does not exist or the operation did not succeed.
/// </returns>
        Task<bool> LikeAnswerAsync(int userId, int answerId);

        /// <summary>
        /// Removes a like from a user from a specific answer.
        /// </summary>
        /// <param name="userId">The ID of the user who is unliking the answer.</param>
        /// <param name="answerId">The ID of the answer to be unliked.</param>
        /// <summary>
/// Removes a previously added like from the specified user on the specified answer.
/// </summary>
/// <param name="userId">ID of the user who is removing their like.</param>
/// <param name="answerId">ID of the answer to remove the like from.</param>
/// <returns>
/// True if a like was found and removed; false if no like existed for the given user and answer.
/// </returns>
        Task<bool> UnlikeAnswerAsync(int userId, int answerId);

        /// <summary>
        /// Removes all likes made by a specific user on any answers belonging to another user.
        /// This is typically called when a user blocks another user.
        /// </summary>
        /// <param name="likerId">The ID of the user whose likes will be removed (the one being blocked).</param>
        /// <param name="blockedId">The ID of the user who owns the answers (the one initiating the block).</param>
        /// <summary>
/// Removes all likes made by one user on every answer owned by another user.
/// </summary>
/// <param name="likerId">ID of the user who created the likes to remove.</param>
/// <param name="blockedId">ID of the user whose answers should have likes removed (typically when they are blocked).</param>
/// <returns>A task that completes when the removal operation finishes.</returns>
        Task RemoveAllLikesFromUserAsync(int likerId, int blockedId);

        /// <summary>
        /// Gets a paginated list of users who have liked a specific answer.
        /// </summary>
        /// <param name="answerId">The ID of the answer.</param>
        /// <param name="pageNumber">The page number for pagination.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <summary>
/// Retrieves a paginated list of users who liked the specified answer.
/// </summary>
/// <param name="answerId">The identifier of the answer whose likers are requested.</param>
/// <param name="pageNumber">1-based page index to return.</param>
/// <param name="pageSize">Maximum number of likers to include in a single page.</param>
/// <returns>A paginated response containing <see cref="LikerDto"/> entries for the requested page, or <c>null</c> if the answer does not exist.</returns>
        Task<PaginatedResponseDto<LikerDto>> GetAnswerLikersAsync(int answerId, int pageNumber, int pageSize);
    }
}
