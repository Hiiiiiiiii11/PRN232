using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObjects
{
    public class UserSurveyAnswer
    {
        [Key]
        public int UserSurveyAnswerID { get; set; }
        public int ResponseID { get; set; }
        public int QuestionID { get; set; }
        public int SelectedAnswerID { get; set; }

        public virtual UserSurveyResponse Response { get; set; }
        public virtual SurveyQuestion Question { get; set; }
        public virtual SurveyAnswer SelectedAnswer { get; set; }
    }
}
