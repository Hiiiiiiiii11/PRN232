using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObjects
{
    public class SurveyAnswer
    {
        [Key]
        public int AnswerID { get; set; }
        public int? QuestionID { get; set; }
        public string AnswerText { get; set; }
        public bool? IsCorrect { get; set; }

        public virtual SurveyQuestion Question { get; set; }
    }
}
