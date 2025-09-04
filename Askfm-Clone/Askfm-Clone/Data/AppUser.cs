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
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public int Coins { get; set; }
        public bool AllowAnonymous { get; set; }

        // Navigation properties
        public ICollection<Question> QuestionsSent { get; set; } = new List<Question>();
        public ICollection<Question> QuestionsReceived { get; set; } = new List<Question>();
        public ICollection<Answer> Answers { get; set; } = new List<Answer>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<Follow> Following { get; set; } = new List<Follow>();
        public ICollection<Follow> Followers { get; set; } = new List<Follow>();
        public ICollection<Block> BlocksMade { get; set; } = new List<Block>();
        public ICollection<Block> BlocksReceived { get; set; } = new List<Block>();
        public ICollection<CoinsTransaction> CoinsTransactions { get; set; } = new List<CoinsTransaction>();

        public ICollection<RefreshTokenInfo> RefreshTokens { get; set; } = new List<RefreshTokenInfo>();

    }
}
