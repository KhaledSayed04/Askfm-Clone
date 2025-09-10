using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Askfm_Clone.Data
{
    public class Answer
    {
        /*
            - Id (PK)
            - QuestionId (FK->Questions)
            - Content (text)
            - CreatedAt
        */
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // --- CORRECTED RELATIONSHIP ---
        // These two properties form a composite foreign key that correctly
        // points to the composite primary key of QuestionRecipient.
        public int QuestionId { get; set; }
        public int ReceptorId { get; set; }
        public QuestionRecipient QuestionRecipient { get; set; }
        // --- END CORRECTION ---

        public int CreatorId { get; set; }
        public AppUser Creator { get; set; }

        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<Like> Likes { get; set; } = new List<Like>();
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}
