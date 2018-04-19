namespace Bspeak.Services.Auth.Models
{
    public class UserModel
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public RegisterType RegisterTye { get; set; }
        public string access_token { get; set; }
        public string client_id { get; set; }
        public string client_secret { get; set; }
    }
}