using DrugUserPreventionUI.Models.Categories;

namespace DrugUserPreventionUI.Models.NewsArticles
{
    public class NewsByCategoryDto
    {
        public CategoryDTO? Category { get; set; }
        public List<NewsArticleDto> Articles { get; set; } = new List<NewsArticleDto>();
        public int TotalCount { get; set; }
    }
}
