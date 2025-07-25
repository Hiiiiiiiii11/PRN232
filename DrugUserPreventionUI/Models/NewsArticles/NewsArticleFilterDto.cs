namespace DrugUserPreventionUI.Models.NewsArticles
{
    public class NewsArticleFilterDto
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public string? SearchKeyword { get; set; }
        public int? CategoryID { get; set; }
        public string? NewsSource { get; set; }
        public string? NewsStatus { get; set; }
        public int? CreatedByID { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
