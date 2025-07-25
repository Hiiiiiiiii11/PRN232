using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObjects
{
    public class SurveyQuestion
    {
        [Key]
        public int QuestionID { get; set; }
        public int? SurveyID { get; set; }
        public string QuestionText { get; set; }
        public string QuestionType { get; set; } // SingleChoice, MultipleChoice, Text

        public virtual Survey Survey { get; set; }
        public virtual ICollection<SurveyAnswer> Answers { get; set; }
    }
}
