using Askfm_Clone.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Askfm_Clone.DTOs.Questions
{
    public class PostQuestionDto
    {
        public int ToUserId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsAnonymous { get; set; }
    }
}
