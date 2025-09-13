using System.ComponentModel.DataAnnotations;

namespace Askfm_Clone.DTOs.Auth
{
    public class LogoutDto
    {

        [Required]
        public string DeviceId { get; set; }
    }
}
