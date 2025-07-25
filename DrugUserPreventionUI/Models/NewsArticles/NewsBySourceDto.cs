namespace DrugUserPreventionUI.Models.NewsArticles
{
    public class NewsBySourceDto
    {
        public string NewsSource { get; set; } = "";
        public List<NewsArticleDto> Articles { get; set; } = new List<NewsArticleDto>();
        public int TotalCount { get; set; }
    }
}
