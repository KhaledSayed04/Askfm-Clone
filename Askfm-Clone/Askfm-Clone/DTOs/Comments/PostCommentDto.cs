using System.ComponentModel.DataAnnotations;

namespace Askfm_Clone.DTOs.Comments
{
    public class PostCommentDto
    {
        [Required]
        public int AnswerId { get; set; }

        [Required]
        [MinLength(1)]
        [MaxLength(500)] // Example max length for a comment
        public string Content { get; set; }
    }
}
