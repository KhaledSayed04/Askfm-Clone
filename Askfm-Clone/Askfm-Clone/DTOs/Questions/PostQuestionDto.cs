using Askfm_Clone.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Askfm_Clone.DTOs.Questions
{
    public class PostQuestionDto
    {
        [Required]
        public int ToUserId { get; set; }
        [Required]
        [MinLength(1)]
        [MaxLength(300)]
        public string Content { get; set; }
        [Required]
        public bool IsAnonymous { get; set; } 
    }
}
