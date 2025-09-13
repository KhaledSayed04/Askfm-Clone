using Askfm_Clone.Data;

namespace Askfm_Clone.DTOs.Answers
{
    public class AnswerDetailsDto
    {
        public int AnswerId { get; init; }
        public int QuestionId { get; init; }
        public int RecipientId { get; init; }
        public int CreatorId { get; init; }
        public string Content { get; init; } = null!;
        public DateTime CreatedAt { get; init; }
        public int LikesCount { get; init; }
        public int CommentsCount { get; init; }
    }
}
