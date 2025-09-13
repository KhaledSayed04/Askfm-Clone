using Askfm_Clone.Data;
using System.ComponentModel.DataAnnotations;

namespace Askfm_Clone.DTOs.Answers
{
    public class PostAnswerDto
    {
        [Required]
        public int QuestionId { get; set; }

        [Required]
        [MinLength(1)]
        [MaxLength(2000)] // Example max length for an answer
        public string Content { get; set; }
    }
}
