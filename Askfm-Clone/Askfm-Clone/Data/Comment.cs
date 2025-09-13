using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Askfm_Clone.Data
{
    public class Comment
    {
        /*
            - Id (PK)
            - AnswerId (FK->Answers)
            - FromUserId (FK->Users, nullable for anonymous)
            - Content (text)
            - CreatedAt
            - IsAnonymous (bool)
         */
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int AnswerId { get; set; }
        public Answer Answer { get; set; }
        public int CreatorId { get; set; }
        public AppUser Creator { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsAnonymous { get; set; }
    }
}
