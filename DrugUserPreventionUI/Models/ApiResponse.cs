namespace DrugUserPreventionUI.Models
{
    public class ApiResponse <T>
    {
        public string message { get; set; }
        public T Data { get; set; }
    }

}
