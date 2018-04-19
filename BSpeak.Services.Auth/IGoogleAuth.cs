using System.Threading.Tasks;
using Xamarin.Forms;

namespace Bspeak.Services.Auth
{
    public interface IGoogleAuth
    {
        Task<AuthResult> Authorize(ContentView authViewContainer);
    }
}