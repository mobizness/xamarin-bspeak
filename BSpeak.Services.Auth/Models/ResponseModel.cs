namespace Bspeak.Services.Auth.Models
{
    public class ResponseModel
    {
        public string Status { get; set; }
        public string Error { get; set; }
        public object Data { get; set; }
        public int TotalPages { get; set; }
        public int TotalRows { get; set; }
        public int PageSize { get; set; }
        public string ReturnMessage { get; set; }
    }
}