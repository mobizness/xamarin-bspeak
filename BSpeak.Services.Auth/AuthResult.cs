using Bspeak.Services.Auth.Models;

namespace Bspeak.Services.Auth
{
    public class AuthResult
    {
        public readonly bool IsAuthenticated;
        public readonly string AccessToken;

        public AuthResult(bool isAuthenticated, string accessToken)
        {
            IsAuthenticated = isAuthenticated;
            AccessToken = accessToken;
        }
    }
}