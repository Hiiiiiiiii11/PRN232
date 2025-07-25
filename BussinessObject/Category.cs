using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObjects
{
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        [Required, MaxLength(200)]
        public string CategoryName { get; set; } = string.Empty;
        public string? CategoryDescription { get; set; }
        public string? ParentCategoryID { get; set; }

        [Required]
        public bool IsActive { get; set; }

        public virtual ICollection<NewsArticle> NewsAticles { get; set; }
    }
}
