using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs.Tags
{
    public class NewsTagDTO
    {
        public int? NewsArticleID { get; set; }
        public int? TagID { get; set; }
        public TagDTO? Tag { get; set; }
    }
}
