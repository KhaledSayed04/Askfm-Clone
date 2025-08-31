using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base_Library.Responses
{
    public record LoginReponse(bool Flag, string Message = null!,
        string? Token = null, string RefreshToken = null!
        );

}
