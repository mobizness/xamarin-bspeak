using System.Threading.Tasks;
using Bspeak.Services.Auth.Models;

namespace Bspeak.Services.Auth
{
    public interface IBspeakAuth
    {
        Task<UserProfile> GetBspeakProfile(RegisterType registerType, string accessToken);
    }

    public enum AuthorizationService
    {
        Facebook,
        Google,
        Mobile
    }
}