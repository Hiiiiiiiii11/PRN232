namespace DrugUserPreventionUI.Configuration
{
    public class ApiConfiguration
    {
        public string BaseUrl { get; set; } = "https://localhost:7045";
        
        public string AuthUrl => $"{BaseUrl}";
        public string UserApiUrl => $"{BaseUrl}/api/User";
        public string CoursesApiUrl => $"{BaseUrl}/api/Courses";
        public string NewsArticlesApiUrl => $"{BaseUrl}/api/NewsArticles";
        public string CategoriesApiUrl => $"{BaseUrl}/api/Categories";
        public string ConsultantApiUrl => $"{BaseUrl}/api/ConsultantUser";
        public string AppointmentApiUrl => $"{BaseUrl}/api/Appointment";
        public string AdminApiUrl => $"{BaseUrl}/api/Admin";
        public string LearningApiUrl => $"{BaseUrl}/api/Learning";
    }
}