namespace Askfm_Clone.DTOs.Questions
{
    public class QuestionRecipientDto
    {
        public int QuestionId { get; set; }
        public string Content { get; set; }
        public int? SenderId { get; set; } // Null if anonymous
        public bool IsAnonymous { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
