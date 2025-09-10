using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Askfm_Clone.DTOs.Auth
{
    public class AccountBaseDto
    {
        [DataType(DataType.EmailAddress)] // UI hint attribute → does not validate, only provides a hint for rendering.
        [EmailAddress] // Validator attribute → checks the property value against a built-in regex for valid email addresses.
        [Required(ErrorMessage = "Email is required.")]
        public string? Email { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password is required.")]
        public string? Password { get; set; }
    }
}
