using Askfm_Clone.Data;
using Askfm_Clone.DTOs;

namespace Askfm_Clone.Services.Contracts
{
    public interface ICommentService
    {
        public Task<bool> OwnComment(int commentId, int userId);
        public Task<int?> AddComment(Comment comment, int userId, int answerId);
        public Task<bool> DeleteComment(int commentId);
        public Task<PaginatedResponseDto<Comment>> GetPaginatedComments(int pageNumber, int pageSize, int answerId);
    }
}
