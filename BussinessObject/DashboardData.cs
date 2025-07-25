using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObjects
{
    public class DashboardData
    {
        [Key]
        public int ID { get; set; }
        public string Metric { get; set; }
        public int Value { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
