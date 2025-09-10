using Askfm_Clone.Data;

namespace Askfm_Clone.DTOs.Answers
{
    public class AnswerDetailsDto
    {
        public Answer Answer { get; set; }
        public QuestionRecipient QuestionRecipient { get; set; }
        public bool HasComments { get; set; }
    }
}
