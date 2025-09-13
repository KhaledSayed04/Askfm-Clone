using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Askfm_Clone.Data
{
    public class Question
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // A question now only knows who sent it.
        // The recipients are handled entirely by the QuestionRecipient join table.
        public int? SenderId { get; set; } // Nullable for truly anonymous questions
        public AppUser? Sender { get; set; }

        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }

        // This flag is now the single source of truth for anonymity.
        public bool IsAnonymous { get; set; }

        // Navigation property to link to all the times this question has been sent.
        public ICollection<QuestionRecipient> Recipients { get; set; } = new List<QuestionRecipient>();
    }
}
