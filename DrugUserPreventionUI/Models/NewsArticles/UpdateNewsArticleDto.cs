using System.ComponentModel.DataAnnotations;

namespace DrugUserPreventionUI.Models.NewsArticles
{
    public class UpdateNewsArticleDto
    {
        public int NewsArticleID { get; set; }

        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [StringLength(500, ErrorMessage = "Tiêu đề không được vượt quá 500 ký tự")]
        public string NewsTitle { get; set; } = "";

        [Required(ErrorMessage = "Tiêu đề phụ là bắt buộc")]
        [StringLength(1000, ErrorMessage = "Tiêu đề phụ không được vượt quá 1000 ký tự")]
        public string Headline { get; set; } = "";

        [Required(ErrorMessage = "Nội dung là bắt buộc")]
        public string NewsContent { get; set; } = "";

        [Required(ErrorMessage = "Nguồn tin là bắt buộc")]
        [StringLength(200, ErrorMessage = "Nguồn tin không được vượt quá 200 ký tự")]
        public string NewsSource { get; set; } = "";

        public int? CategoryID { get; set; }
        public string? NewsStatus { get; set; }
        public List<int>? TagIds { get; set; }
    }
}
