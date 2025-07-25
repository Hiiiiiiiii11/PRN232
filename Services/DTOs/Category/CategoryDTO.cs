using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BussinessObjects;

namespace Services.DTOs.Category
{
    public class CategoryDTO : BasePaginationDto
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string CategoryDescription { get; set; }
        public int? ParentCategoryID { get; set; } // Thêm ParentCategoryID vào DTO
        public int NewsCount { get; set; }

        public ICollection<NewsArticle>? NewsArticles { get; set; }
    }
}
