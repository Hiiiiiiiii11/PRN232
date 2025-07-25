namespace DrugUserPreventionUI.Models.NewsArticles
{
    public class NewsArticleStatsDto
    {
        public int TotalArticles { get; set; }
        public int ActiveArticles { get; set; }
        public int InactiveArticles { get; set; }
        public int TodayArticles { get; set; }
        public int ThisWeekArticles { get; set; }
        public int ThisMonthArticles { get; set; }
        public List<NewsArticleDto> RecentArticles { get; set; } = new List<NewsArticleDto>();
    }
}
