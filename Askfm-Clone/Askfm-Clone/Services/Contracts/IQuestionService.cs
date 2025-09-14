using Askfm_Clone.Data;
using Askfm_Clone.DTOs;
using Askfm_Clone.DTOs.Questions;
using System.Linq.Expressions;

namespace Askfm_Clone.Services.Contracts
{
    public interface IQuestionService
    {
        public Task<int?> CreateQuestion(Question question, int targetUserId);
        public Task<int?> CreateRandomQuestion(Question question, int numberOfRecipients);
        public Task<Question?> GetQuestionById(int questionId);
        public Task<PaginatedResponseDto<Question>> GetQuestions(int pageNumber, int pageSize, Expression<Func<Question, bool>> predicate);
        public Task<PaginatedResponseDto<QuestionRecipientDto>> GetReceivedQuestionsAsync(int userId, bool hasAnswered, int pageNumber, int pageSize);
        public Task<PaginatedResponseDto<QuestionRecipientDto>> GetQuestionsAsync(bool hasAnswered, int pageNumber, int pageSize);
        public Task<bool> DeleteQuestion(int questionId);
    }
}
