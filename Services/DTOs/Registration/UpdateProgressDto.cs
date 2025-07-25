using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs.Registration
{
    public class UpdateProgressDto
    {
        [Required]
        public int RegistrationID { get; set; }

        [Range(0, 100)]
        public double Progress { get; set; }

        public bool Completed { get; set; }
    }
}
