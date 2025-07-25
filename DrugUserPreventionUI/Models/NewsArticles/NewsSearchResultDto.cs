namespace DrugUserPreventionUI.Models.NewsArticles
{
    public class NewsSearchResultDto
    {
        public List<NewsArticleDto> Articles { get; set; } = new List<NewsArticleDto>();
        public int TotalCount { get; set; }
        public string SearchKeyword { get; set; } = "";
        public TimeSpan SearchTime { get; set; }
    }
}
