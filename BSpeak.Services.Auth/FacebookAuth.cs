using System;
using System.Threading.Tasks;
using Bspeak.Core;
using Xamarin.Forms;

namespace Bspeak.Services.Auth
{
    public class FacebookAuth : IFacebookAuth
    {
        private readonly WebView _authWebView;
        private ContentView _authViewContainer;
        private TaskCompletionSource<AuthResult> _authWebViewWaiter;
        private View _storedContent;

        public FacebookAuth()
        {
            var apiRequest =
                $"https://www.facebook.com/v2.8/dialog/oauth?client_id={AppConstants.FacebookClientId}&display=popup&response_type=token&redirect_uri={AppConstants.FacebookRedirectUrl}";

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
            try
            {
                var webAuthResult = await _authWebViewWaiter.Task;

                _storedContent = null;
                _authViewContainer = null;
                _authWebViewWaiter = null;
                _authWebView.Navigated -= OnAuthWebViewNavigated;

                return webAuthResult;
            }
            catch (TaskCanceledException e)
            {
            }

            return null;
        }

        public void DismissAuthorize()
        {
            _authWebViewWaiter?.TrySetCanceled();
            _authWebViewWaiter = null;
        }

        private void OnAuthWebViewNavigated(object sender, WebNavigatedEventArgs e)
        {
            if (e.Url.Contains("access_token"))
            {
                var accessToken = ExtractAccessTokenFromUrl(e.Url);

                _authViewContainer.Content = _storedContent;

                _authWebViewWaiter.TrySetResult(!string.IsNullOrWhiteSpace(accessToken)
                    ? new AuthResult(true, accessToken)
                    : new AuthResult(false, null));
            }
        }

        private string ExtractAccessTokenFromUrl(string url)
        {
            var query = new Uri(url).Fragment;
            if (query.Contains("access_token="))
            {
                var accessTokenStart = query.IndexOf("access_token=") + "access_token=".Length;
                var accessTokenEnd = query.IndexOf('&', accessTokenStart);
                var accessTokenLength = (accessTokenEnd > accessTokenStart ? accessTokenEnd : query.Length) -
                                        accessTokenStart;
                //var at = url.Replace($"{AppConstants.FacebookRedirectUrl}#access_token=", "");

                var accessToken =
                    query.Substring(accessTokenStart,
                        accessTokenLength); //at.Remove(at.IndexOf("&expires_in=", StringComparison.Ordinal));

                return accessToken;
            }

            return string.Empty;
        }
    }
}