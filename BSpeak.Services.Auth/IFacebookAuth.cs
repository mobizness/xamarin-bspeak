using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Bspeak.Services.Auth
{
    public interface IFacebookAuth
    {
        Task<AuthResult> Authorize(ContentView authViewContainer);
        void DismissAuthorize();
    }
}
