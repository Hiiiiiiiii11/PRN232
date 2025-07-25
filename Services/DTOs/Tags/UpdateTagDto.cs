using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs.Tags
{
    public class UpdateTagDto
    {
        public int TagID { get; set; }
        public string TagName { get; set; }
        public string Note { get; set; }
    }
}
