using System.ComponentModel.DataAnnotations;

namespace Askfm_Clone.DTOs.Auth
{
    public class LogoutAllDto
    {
        [Required]
        public int UserId { get; set; }
    }
}
