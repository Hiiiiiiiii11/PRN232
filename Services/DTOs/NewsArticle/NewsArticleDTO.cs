using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.DTOs.Category;
using Services.DTOs.Tags;
using Services.DTOs.User;

namespace Services.DTOs.NewArticle
{
    public class NewsArticleDto : BasePaginationDto
    {
        public int NewsArticleID { get; set; }
        public string NewsTitle { get; set; }
        public string Headline { get; set; }
        public DateTime CreatedDate { get; set; }
        public string NewsContent { get; set; }
        public string NewsSource { get; set; }
        public int? CategoryID { get; set; } // Chứa tên danh mục thay vì CategoryID
        public string? NewsStatus { get; set; }
        public int? CreatedByID { get; set; } // Tên người tạo bài viết
        public int? UpdatedByID { get; set; } // Tên người cập nhật bài viết
        public DateTime ModifiedDate { get; set; }

        public UserDTO? CreatedBy { get; set; }
        public UserDTO? UpdatedBy { get; set; }
        public virtual CategoryDTO? Category { get; set; }

        // Many-to-many with Tag, so we define the list of NewsTags here
        public ICollection<NewsTagDTO>? NewsTags { get; set; } // Adjust this based on whether you want the entity or DTO
    }
}
