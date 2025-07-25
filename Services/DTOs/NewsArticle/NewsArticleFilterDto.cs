using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs.NewArticle
{
    public class NewsArticleFilterDto : BasePaginationDto
    {
        public string? SearchKeyword { get; set; }
        public int? CategoryID { get; set; }
        public string? NewsSource { get; set; }
        public string? NewsStatus { get; set; }
        public int? CreatedByID { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
