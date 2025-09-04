using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base_Library.DTOs
{
    public class LoginDto: AccountBaseDto
    {
        public string? DeviceId { get; set; }
    }
}
