namespace DrugUserPreventionUI.Models.Courses
{
    public class CourseDto
    {
        public int CourseID { get; set; }
        public string CourseName { get; set; }
        public string CourseDescription { get; set; }
        public string? ThumbnailURL { get; set; } =
            "https://cdn.pixabay.com/photo/2013/07/13/10/12/no-drugs-156771_1280.png";
        public string Status { get; set; }
        public int? CreatedByID { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
