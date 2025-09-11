using Askfm_Clone.Data;
using Askfm_Clone.DTOs;

namespace Askfm_Clone.Services.Contracts
{
    public interface ICommentService
    {
        /// <summary>
/// Asynchronously checks whether the specified user is the owner of the given comment.
/// </summary>
/// <param name="commentId">The identifier of the comment to check.</param>
/// <param name="userId">The identifier of the user to verify as the owner.</param>
/// <returns>A task that resolves to true if the user owns the comment; otherwise false.</returns>
public Task<bool> OwnComment(int commentId, int userId);
        /// <summary>
/// Asynchronously creates a new comment on an answer.
/// </summary>
/// <param name="comment">The Comment entity to create (content and metadata).</param>
/// <param name="userId">ID of the user creating the comment.</param>
/// <param name="answerId">ID of the answer the comment is attached to.</param>
/// <returns>
/// A task that resolves to the new comment's ID, or null if creation failed.
/// </returns>
public Task<int?> AddComment(Comment comment, int userId, int answerId);
        /// <summary>
/// Asynchronously deletes the comment with the specified identifier.
/// </summary>
/// <param name="commentId">Identifier of the comment to delete.</param>
/// <returns>
/// A task that resolves to true if the comment was found and removed; otherwise false.
/// </returns>
public Task<bool> DeleteComment(int commentId);
        /// <summary>
/// Retrieves a paginated list of comments for a specific answer.
/// </summary>
/// <param name="pageNumber">The page index to retrieve.</param>
/// <param name="pageSize">Number of comments per page.</param>
/// <param name="answerId">Identifier of the answer whose comments are requested.</param>
/// <returns>
/// A <see cref="PaginatedResponseDto{Comment}"/> containing the page of comments and pagination metadata.
/// </returns>
public Task<PaginatedResponseDto<Comment>> GetPaginatedComments(int pageNumber, int pageSize, int answerId);
    }
}
