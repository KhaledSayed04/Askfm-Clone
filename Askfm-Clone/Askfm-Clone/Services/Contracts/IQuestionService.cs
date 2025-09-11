using Askfm_Clone.Data;
using Askfm_Clone.DTOs;
using Askfm_Clone.DTOs.Questions;
using System.Linq.Expressions;

namespace Askfm_Clone.Services.Contracts
{
    public interface IQuestionService
    {
        /// <summary>
/// Asynchronously creates and persists a question addressed to a specific user.
/// </summary>
/// <param name="question">The question to create (must contain the question content and any metadata to store).</param>
/// <param name="targetUserId">The identifier of the user who should receive the question.</param>
/// <returns>A <see cref="Task{Question}"/> that completes with the created <see cref="Question"/>, including any generated identifiers or persisted state.</returns>
public Task<Question> CreateQuestion(Question question, int targetUserId);
        /// <summary>
/// Creates a question that will be delivered to a number of randomly chosen recipients.
/// </summary>
/// <param name="question">The question to create; may contain content and metadata used for the created entity.</param>
/// <param name="numberOfRecipients">The number of random recipients to assign the question to. Must be a positive value.</param>
/// <returns>A task that resolves to the created <see cref="Question"/> with recipients and any assigned identifiers populated.</returns>
public Task<Question> CreateRandomQuestion(Question question, int numberOfRecipients);
        /// <summary>
/// Retrieves a question by its identifier.
/// </summary>
/// <param name="questionId">The unique identifier of the question to retrieve.</param>
/// <returns>A task that resolves to the <see cref="Question"/> with the specified id, or <c>null</c> if no such question exists.</returns>
public Task<Question?> GetQuestionById(int questionId);
        /// <summary>
/// Retrieves a paginated list of questions matching the provided filter.
/// </summary>
/// <param name="pageNumber">The page index to return (pagination anchor).</param>
/// <param name="pageSize">The number of items per page.</param>
/// <param name="predicate">An expression used to filter questions; applied to select which questions are included.</param>
/// <returns>A <see cref="PaginatedResponseDto{Question}"/> containing the questions for the requested page and pagination metadata.</returns>
public Task<PaginatedResponseDto<Question>> GetQuestions(int pageNumber, int pageSize, Expression<Func<Question, bool>> predicate);
        /// <summary>
/// Retrieves a paginated list of questions that were received by a specific user.
/// </summary>
/// <param name="userId">ID of the user who received the questions.</param>
/// <param name="hasAnswered">
/// If true, only include questions the user has answered; if false, only include unanswered questions.
/// </param>
/// <param name="pageNumber">1-based page index to return.</param>
/// <param name="pageSize">Number of items per page.</param>
/// <returns>
/// A task that resolves to a paginated response DTO containing QuestionRecipientDto entries for the requested page.
/// </returns>
public Task<PaginatedResponseDto<QuestionRecipientDto>> GetReceivedQuestionsAsync(int userId, bool hasAnswered, int pageNumber, int pageSize);
        /// <summary>
/// Asynchronously deletes the question with the specified identifier.
/// </summary>
/// <param name="questionId">Identifier of the question to delete.</param>
/// <returns>
/// A task that resolves to true if the question was found and deleted; otherwise false.
/// </returns>
public Task<bool> DeleteQuestion(int questionId);
    }
}
