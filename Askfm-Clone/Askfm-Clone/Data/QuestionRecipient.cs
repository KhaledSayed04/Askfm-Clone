namespace Askfm_Clone.Data
{
    public class QuestionRecipient
    {
        public int ReceptorId { get; set; }
        public AppUser Receptor { get; set; }
        
        public int QuestionId { get; set; }
        public Question Question { get; set; }

        public Answer? Answer { get; set; }
    }
}
