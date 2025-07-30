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

    public static class ApiUrlHelper
    {
        private static ApiConfiguration? _instance;
        
        public static void Configure(ApiConfiguration config)
        {
            _instance = config;
        }
        
        public static string GetBaseUrl()
        {
            return _instance?.BaseUrl ?? "https://localhost:7045";
        }
        
        public static string GetApiUrl(string endpoint)
        {
            var baseUrl = GetBaseUrl();
            return $"{baseUrl}/api/{endpoint.TrimStart('/')}";
        }
        
        public static string GetAuthUrl() => _instance?.AuthUrl ?? "https://localhost:7045";
        public static string GetUserApiUrl() => _instance?.UserApiUrl ?? "https://localhost:7045/api/User";
        public static string GetCoursesApiUrl() => _instance?.CoursesApiUrl ?? "https://localhost:7045/api/Courses";
        public static string GetNewsArticlesApiUrl() => _instance?.NewsArticlesApiUrl ?? "https://localhost:7045/api/NewsArticles";
        public static string GetCategoriesApiUrl() => _instance?.CategoriesApiUrl ?? "https://localhost:7045/api/Categories";
        public static string GetConsultantApiUrl() => _instance?.ConsultantApiUrl ?? "https://localhost:7045/api/ConsultantUser";
        public static string GetAppointmentApiUrl() => _instance?.AppointmentApiUrl ?? "https://localhost:7045/api/Appointment";
        public static string GetAdminApiUrl() => _instance?.AdminApiUrl ?? "https://localhost:7045/api/Admin";
        public static string GetLearningApiUrl() => _instance?.LearningApiUrl ?? "https://localhost:7045/api/Learning";
    }
}