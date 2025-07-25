using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObjects
{
    public class Survey
    {
        [Key]
        public int SurveyID { get; set; }
        public string Title { get; set; }
        public string Type { get; set; } // ASSIST, CRAFFT, PreProgram, PostProgram
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ThumbnailURL { get; set; } // URL of the survey's thumbnail image

        public virtual ICollection<SurveyQuestion> Questions { get; set; }
    }
}
