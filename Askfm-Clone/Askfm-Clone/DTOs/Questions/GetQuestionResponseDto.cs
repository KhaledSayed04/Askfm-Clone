using Askfm_Clone.Data;

namespace Askfm_Clone.DTOs.Questions
{
    public class GetQuestionResponseDto
    {
        public int Id { get; set; }
        public int? SenderUserId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsAnonymous { get; set; }
    }
}
