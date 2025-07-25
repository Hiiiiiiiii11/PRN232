namespace DrugUserPreventionUI.Models.Tags
{
    public class NewsTagDTO
    {
        public int NewsTagID { get; set; }
        public int NewsArticleID { get; set; }
        public int TagID { get; set; }
        public TagDTO? Tag { get; set; }
    }
}
