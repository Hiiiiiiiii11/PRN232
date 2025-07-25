using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs.User
{
    public class TokenResponseDto
    {
        public required string AccessToken { get; set; }
    }
}
