using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Askfm_Clone.Data
{
    public class AppUser
    {
        /*
            - Id (PK)
            - Username
            - Email
            - PasswordHash
            - AvatarUrl
            - Settings (e.g., AllowAnonymousQuestions, AllowAnonymousComments)
            - CreatedAt
         */
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]//auto-generated
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public bool AllowAnonymous { get; set; } = true; // AllowAnonymousQuestions and AllowAnonymousComments
        public int Coins { get; set; } = 0;
        public List<Question> QuestionsSent { get; set; }     // As sender
        public List<Question> QuestionsReceived { get; set; } // As receiver
        public List<Answer> Answers { get; set; }
        public List<CoinsTransaction> CoinsTransactions { get; set; }
        public List<Comment> Comments { get; set; }
        public List<Like> Likes { get; set; }

        public List<Follow> Following { get; set; }   // users I follow
        public List<Follow> Followers { get; set; }   // users following me

        public List<Block> BlocksMade { get; set; }   // users I blocked
        public List<Block> BlocksReceived { get; set; } // users that blocked me
    }
}
