using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs
{
    public class PagingRequest
    {
        public int index { get; set; } = 1;
        public int pageSize { get; set; } = 10;
    }
}
