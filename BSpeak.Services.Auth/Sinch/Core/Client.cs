using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Sinch.Core
{
	public class Client : IDisposable
	{
		private string _applicationKey;

		private string _applicationSecret;

		private Security security;

		public Client()
		{
			throw new Exception("Client must be initialized with kay and secret");
		}

		public Client(string applicationKey, string applicationSecret)
		{
			this._applicationKey = applicationKey;
			this._applicationSecret = applicationSecret;
			this.security = new Security(applicationKey, applicationSecret);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
		}

		public async Task<HttpResponseMessage> GetAsync(string url)
		{
			HttpResponseMessage async;
			UriBuilder uriBuilder = new UriBuilder(url);
			using (HttpClient httpClient = new HttpClient())
			{
				string str = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
				httpClient.DefaultRequestHeaders.Add("x-timestamp", str);
				httpClient.DefaultRequestHeaders.Add("Authorization", string.Concat("application ", this.security.SignGetRequest("GET", uriBuilder.Path, str)));
				async = await httpClient.GetAsync(url);
			}
			return async;
		}

		public async Task<HttpResponseMessage> PostAsJsonAsync<T>(string url, T value)
		{
			HttpResponseMessage httpResponseMessage;
			UriBuilder uriBuilder = new UriBuilder(url);
			using (HttpClient httpClient = new HttpClient())
			{
				string str = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
				httpClient.DefaultRequestHeaders.Add("x-timestamp", str);
				httpClient.DefaultRequestHeaders.Add("Authorization", string.Concat("application ", this.security.SignRequest("POST", JsonConvert.SerializeObject(value), uriBuilder.Path, str)));
				//httpResponseMessage = await HttpClientExtensions.PostAsJsonAsync<T>(httpClient, url, value);
                string json = JsonConvert.SerializeObject(value);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
             
                httpResponseMessage = await httpClient.PostAsync(url, content);
            }
			return httpResponseMessage;
		}

		public async Task<HttpResponseMessage> PutAsJsonAsync<T>(string url, T value)
		{
			HttpResponseMessage httpResponseMessage;
			UriBuilder uriBuilder = new UriBuilder(url);
			using (HttpClient httpClient = new HttpClient())
			{
				string str = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
				httpClient.DefaultRequestHeaders.Add("x-timestamp", str);
				httpClient.DefaultRequestHeaders.Add("Authorization", string.Concat("application ", this.security.SignRequest("PUT", JsonConvert.SerializeObject(value), uriBuilder.Path, str)));
                //httpResponseMessage = await HttpClientExtensions.PutAsJsonAsync<T>(httpClient, url, value);
                string json = JsonConvert.SerializeObject(value);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                httpResponseMessage =await httpClient.PutAsync(url, content);
                            //    httpResponseMessage = await httpClient.PostAsync(url, content);
            }
			return httpResponseMessage;
		}
	}
}