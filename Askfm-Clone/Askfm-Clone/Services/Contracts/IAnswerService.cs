using Askfm_Clone.Data;
using Askfm_Clone.DTOs;
using Askfm_Clone.DTOs.Answers;

namespace Askfm_Clone.Services.Contracts
{
    public enum OrderAnswersChoice
    {
        Recent,
        Popular
    }
    public interface IAnswerService
    {
        /// <summary>
/// Determines whether the specified answer is owned by the given user.
/// </summary>
/// <param name="answerId">ID of the answer to check.</param>
/// <param name="userId">ID of the user to verify ownership for.</param>
/// <returns>A task that resolves to true if the answer exists and is owned by the user; otherwise false.</returns>
public Task<bool> OwnAnswer(int answerId, int userId);
        /// <summary>
/// Creates a new answer for the specified question authored by the given user.
/// </summary>
/// <param name="answer">The Answer entity to add (should contain the answer content and metadata).</param>
/// <param name="questionId">ID of the question being answered.</param>
/// <param name="userId">ID of the user creating the answer.</param>
/// <returns>
/// A task that completes with the newly created answer's ID, or <c>null</c> if creation failed.
/// </returns>
public Task<int?> AddAnswer(Answer answer, int questionId, int userId);
        /// <summary>
/// Deletes the answer with the specified identifier.
/// </summary>
/// <param name="answerId">The identifier of the answer to delete.</param>
/// <returns>A task that resolves to true if the answer was successfully deleted; otherwise false.</returns>
public Task<bool> DeleteAnswer(int answerId);
        /// <summary>
/// Retrieves a paginated list of answer details for a specific user.
/// </summary>
/// <param name="pageNumber">The index of the page to retrieve.</param>
/// <param name="pageSize">The number of answers per page.</param>
/// <param name="userId">The ID of the user whose answers are requested.</param>
/// <param name="order">The ordering to apply to results (Recent or Popular).</param>
/// <returns>A task that resolves to a paginated response containing AnswerDetailsDto entries for the requested page.</returns>
public Task<PaginatedResponseDto<AnswerDetailsDto>> GetPaginatedAnswers(int pageNumber, int pageSize, int userId, OrderAnswersChoice order);
    }
}
