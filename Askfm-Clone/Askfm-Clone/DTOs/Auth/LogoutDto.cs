using System.ComponentModel.DataAnnotations;

namespace Askfm_Clone.DTOs.Auth
{
    public class LogoutDto : LogoutAllDto
    {

        [Required]
        public string DeviceId { get; set; }
    }
}
