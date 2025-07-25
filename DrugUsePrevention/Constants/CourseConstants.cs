namespace DrugUsePrevention.Constants
{
    public static class CourseConstants
    {
        public static class TargetGroups
        {
            public const string Students = "Học sinh";
            public const string UniversityStudents = "Sinh viên";
            public const string Parents = "Phụ huynh";
            public const string Teachers = "Giáo viên";
            public const string GeneralPublic = "Cộng đồng";
        }

        public static class AgeGroups
        {
            public const string Elementary = "Tiểu học (6-11 tuổi)";
            public const string MiddleSchool = "Trung học cơ sở (12-15 tuổi)";
            public const string HighSchool = "Trung học phổ thông (16-18 tuổi)";
            public const string University = "Đại học (19-25 tuổi)";
            public const string Adults = "Người lớn (25+ tuổi)";
        }

        public static class ContentTypes
        {
            public const string Video = "Video";
            public const string Text = "Text";
            public const string Quiz = "Quiz";
            public const string Document = "Document";
            public const string Audio = "Audio";
            public const string Interactive = "Interactive";
        }

        public static class Roles
        {
            public const string Guest = "Guest";
            public const string Member = "Member";
            public const string Staff = "Staff";
            public const string Consultant = "Consultant";
            public const string Manager = "Manager";
            public const string Admin = "Admin";
        }

        public static readonly List<string> AllTargetGroups = new()
        {
            TargetGroups.Students,
            TargetGroups.UniversityStudents,
            TargetGroups.Parents,
            TargetGroups.Teachers,
            TargetGroups.GeneralPublic
        };

        public static readonly List<string> AllAgeGroups = new()
        {
            AgeGroups.Elementary,
            AgeGroups.MiddleSchool,
            AgeGroups.HighSchool,
            AgeGroups.University,
            AgeGroups.Adults
        };

        public static readonly List<string> AllContentTypes = new()
        {
            ContentTypes.Video,
            ContentTypes.Text,
            ContentTypes.Quiz,
            ContentTypes.Document,
            ContentTypes.Audio,
            ContentTypes.Interactive
        };
    }
}
