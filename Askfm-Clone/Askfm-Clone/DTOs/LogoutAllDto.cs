using System.ComponentModel.DataAnnotations;

namespace Askfm_Clone.DTOs
{
    public class LogoutAllDto 
    {
        [Required]
        public int UserId { get; set; }
    }
}
