using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Bspeak.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace Bspeak.Services.Auth
{
    public class GoogleAuth : IGoogleAuth
    {
        private readonly WebView _authWebView;
        private ContentView _authViewContainer;
        private TaskCompletionSource<AuthResult> _authWebViewWaiter;
        private View _storedContent;

        public GoogleAuth()
        {
            var apiRequest =
                $"https://accounts.google.com/o/oauth2/v2/auth?response_type=code&scope=openid&redirect_uri={AppConstants.GoogleRedirectUrl}&client_id={AppConstants.GoogleClientId}";

            _authWebView = new WebView
            {
                Source = apiRequest
            };
        }

        public async Task<AuthResult> Authorize(ContentView authViewContainer)
        {
            if (authViewContainer == null)
                throw new ArgumentNullException();

            _authViewContainer = authViewContainer;
            _authWebView.Navigated += OnAuthWebViewNavigated;
            _storedContent = authViewContainer.Content;

            _authWebViewWaiter?.TrySetCanceled();
            _authWebViewWaiter = new TaskCompletionSource<AuthResult>();

            authViewContainer.Content = _authWebView;

            var webAuthResult = await _authWebViewWaiter.Task;

            _storedContent = null;
            _authViewContainer = null;
            _authWebView.Navigated -= OnAuthWebViewNavigated;

            return webAuthResult;
        }

        private async void OnAuthWebViewNavigated(object sender, WebNavigatedEventArgs e)
        {
            var accessToken = await ExtractAccessTokenFromUrl(e.Url);

            _authViewContainer.Content = _storedContent;

            _authWebViewWaiter.TrySetResult(!string.IsNullOrWhiteSpace(accessToken)
                ? new AuthResult(true, accessToken)
                : new AuthResult(false, null));
        }

        private async Task<string> ExtractAccessTokenFromUrl(string url)
        {
            if (url.Contains("code="))
            {
                var attributes = url.Split('&');
                var code = attributes.FirstOrDefault(s => s.Contains("code=")).Split('=')[1];
                try
                {
                    var requestUrl =
                        "https://www.googleapis.com/oauth2/v4/token"
                        + "?code=" + code
                        + "&client_id=" + AppConstants.GoogleClientId
                        + "&client_secret=" + AppConstants.GoogleClientSecret
                        + "&redirect_uri=" + AppConstants.GoogleRedirectUrl
                        + "&grant_type=authorization_code";

                    var httpClient = new HttpClient();

                    var response = await httpClient.PostAsync(requestUrl, null);

                    var json = await response.Content.ReadAsStringAsync();

                    var accessToken = JsonConvert.DeserializeObject<JObject>(json).Value<string>("access_token");

                    return accessToken;
                }
                catch (Exception ex)
                {
                    if(Debugger.IsAttached)
                        Debugger.Break();
                }
            }

            return "";
        }
    }
}