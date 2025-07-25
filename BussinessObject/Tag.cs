using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObjects
{
    public class Tag
    {
        [Key]
        public int TagID { get; set; }

        [Required, MaxLength(200)]
        public string TagName { get; set; }

        [Required]
        public string Note { get; set; } = string.Empty;

        public virtual ICollection<NewsTag> NewsTags { get; set; }
    }
}
