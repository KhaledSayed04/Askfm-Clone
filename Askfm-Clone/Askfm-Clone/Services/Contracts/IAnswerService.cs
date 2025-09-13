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
        public Task<bool> OwnAnswer(int answerId, int userId);
        public Task<int?> AddAnswer(Answer answer, int questionId, int userId);
        public Task<bool> DeleteAnswer(int answerId);
        public Task<PaginatedResponseDto<AnswerDetailsDto>> GetPaginatedAnswers(int pageNumber, int pageSize, int userId, OrderAnswersChoice order);
    }
}
