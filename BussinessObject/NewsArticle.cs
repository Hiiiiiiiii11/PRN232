using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObjects
{
    public class NewsArticle
    {
        [Key]
        public int NewsArticleID { get; set; }

        [Required, MaxLength(200)]
        public string NewsAticleName { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Headline { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public string NewsContent { get; set; } = string.Empty;

        [Required]
        public string NewsSource { get; set; } = string.Empty;

        [Required]
        public int? CategoryID { get; set; }

        [Required, MaxLength(200)]
        public string NewsStatus { get; set; } = string.Empty;

        public int? CreatedByID { get; set; }

        public int? UpdatedByID { get; set; }
        public DateTime? ModifiedDate { get; set; }

        [ForeignKey("CategoryID")]
        public virtual Category? Category { get; set; }

        [ForeignKey("CreatedByID")]
        public virtual User? CreatedBy { get; set; }

        [ForeignKey("UpdatedByID")]
        public virtual User? UpdatedBy { get; set; }

        public virtual ICollection<NewsTag>? NewsTags { get; set; }
    }
}
