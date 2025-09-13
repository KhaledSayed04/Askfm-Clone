using System.ComponentModel.DataAnnotations;

namespace Askfm_Clone.DTOs.Questions
{
    public class PostRandomQuestionDto
    {
        [Required]
        [MinLength(1)]
        [MaxLength(1000)]
        public string Content { get; set; }

        [Required]
        [Range(1, 20, ErrorMessage = "You can send a random question to between 1 and 20 users.")]
        public int NumberOfRecipients { get; set; }
        [Required]
        public bool IsAnonymous { get; set; }
    }
}
