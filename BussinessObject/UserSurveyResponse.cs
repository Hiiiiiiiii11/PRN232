using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObjects
{
    public class UserSurveyResponse
    {
        [Key]
        public int ResponseID { get; set; }
        public int UserID { get; set; }
        public int SurveyID { get; set; }
        public DateTime CompletedAt { get; set; }
        public string RiskLevel { get; set; } // Low, Moderate, High
        public string Recommendation { get; set; }

        public virtual User User { get; set; }
        public virtual Survey Survey { get; set; }
        public virtual ICollection<UserSurveyAnswer> Answers { get; set; }
    }
}
