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
        /// <returns>A boolean indicating if the operation was successful. Returns false if the answer does not exist.</returns>
        Task<bool> LikeAnswerAsync(int userId, int answerId);

        /// <summary>
        /// Removes a like from a user from a specific answer.
        /// </summary>
        /// <param name="userId">The ID of the user who is unliking the answer.</param>
        /// <param name="answerId">The ID of the answer to be unliked.</param>
        /// <returns>A boolean indicating if the operation was successful. Returns false if the like did not exist.</returns>
        Task<bool> UnlikeAnswerAsync(int userId, int answerId);

        /// <summary>
        /// Removes all likes made by a specific user on any answers belonging to another user.
        /// This is typically called when a user blocks another user.
        /// </summary>
        /// <param name="likerId">The ID of the user whose likes will be removed (the one being blocked).</param>
        /// <param name="blockedId">The ID of the user who owns the answers (the one initiating the block).</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task RemoveAllLikesFromUserAsync(int likerId, int blockedId);

        /// <summary>
        /// Gets a paginated list of users who have liked a specific answer.
        /// </summary>
        /// <param name="answerId">The ID of the answer.</param>
        /// <param name="pageNumber">The page number for pagination.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>A paginated response DTO containing the likers' information, or null if the answer does not exist.</returns>
        Task<PaginatedResponseDto<LikerDto>> GetAnswerLikersAsync(int answerId, int pageNumber, int pageSize);
    }
}
