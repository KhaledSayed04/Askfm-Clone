using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Askfm_Clone.DTOs.Auth
{
    public class RegisterDto : AccountBaseDto
    {
        [Required]
        [MinLength(8)]
        [MaxLength(50)]
        public string? Name { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string? ConfirmPassword { get; set; }

    }
}
