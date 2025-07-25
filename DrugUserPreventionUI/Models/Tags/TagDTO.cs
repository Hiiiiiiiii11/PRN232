namespace DrugUserPreventionUI.Models.Tags
{
    public class TagDTO
    {
        public int TagID { get; set; }
        public string TagName { get; set; } = "";
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
