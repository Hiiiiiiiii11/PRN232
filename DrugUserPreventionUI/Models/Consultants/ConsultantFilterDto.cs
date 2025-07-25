using DrugUserPreventionUI.Models.Common;

namespace DrugUserPreventionUI.Models.Consultants
{
    public class ConsultantFilterDto : BasePaginationDto
    {
        public string? SearchKeyword { get; set; }
        public string? Specialty { get; set; }
        public string? Status { get; set; } = "Active";
    }
}
