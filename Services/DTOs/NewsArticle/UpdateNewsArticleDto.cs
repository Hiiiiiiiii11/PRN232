using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs.NewArticle
{
    public class UpdateNewsArticleDto
    {
        public int NewsArticleID { get; set; }
        public string NewsTitle { get; set; }
        public string Headline { get; set; }
        public string NewsContent { get; set; }
        public string NewsSource { get; set; }
        public int? CategoryID { get; set; }
        public string? NewsStatus { get; set; }
        public List<int>? TagIds { get; set; } // For news-tag relationship
    }
}
