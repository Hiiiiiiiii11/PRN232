using DrugUserPreventionUI.Models.Categories;
using DrugUserPreventionUI.Models.Tags;
using DrugUserPreventionUI.Models.Users;

namespace DrugUserPreventionUI.Models.NewsArticles
{
    public class NewsArticleDto
    {
        public int NewsArticleID { get; set; }
        public string NewsTitle { get; set; } = "";
        public string Headline { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public string NewsContent { get; set; } = "";
        public string NewsSource { get; set; } = "";
        public int? CategoryID { get; set; }
        public string? NewsStatus { get; set; }
        public int? CreatedByID { get; set; }
        public int? UpdatedByID { get; set; }
        public DateTime ModifiedDate { get; set; }

        // Navigation properties
        public UserDTO? CreatedBy { get; set; }
        public UserDTO? UpdatedBy { get; set; }
        public CategoryDTO? Category { get; set; }
        public ICollection<NewsTagDTO>? NewsTags { get; set; }
    }
}
