using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Bspeak.Core;
using Bspeak.Services.Auth.Models;
using Newtonsoft.Json;

namespace Bspeak.Services.Auth
{
    public class BspeakAuth : IBspeakAuth
    {
        public async Task<UserProfile> GetBspeakProfile(RegisterType registerType, string accessToken)
        {
            var BspeakAccessToken = await GetBspeakToken(accessToken, registerType);
            if (!string.IsNullOrWhiteSpace(BspeakAccessToken))
            {
                var BspeakProfile = await GetBspeakProfile(BspeakAccessToken);
                if (BspeakProfile != null)
                {
                    BspeakProfile.Token = BspeakAccessToken;
                    return BspeakProfile;
                }
            }
            return null;
        }

        private async Task<string> GetBspeakToken(string accessToken, RegisterType registerType)
        {
            try
            {
                var requestUrl = AppConstants.BaseUrl + "/api/Account/Register";
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("client_id", "Android");
                    client.DefaultRequestHeaders.Add("client_secret", "abc@123");
                    //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    var data = new UserModel
                    {
                        UserName = registerType==RegisterType.FLASHCARD?accessToken:null,
                        access_token = registerType == RegisterType.FLASHCARD ? null : accessToken,
                        RegisterTye = registerType,
                        client_id = "Android",
                        client_secret = "abc@123"
                    };
                    var jsonRequest = JsonConvert.SerializeObject(data);
                    var content = new StringContent(jsonRequest, Encoding.UTF8, "text/json");
                    var result = await client.PostAsync(requestUrl, content);
                    var responseData = await result.Content.ReadAsStringAsync();
                    var responseModel = JsonConvert.DeserializeObject<string>(responseData);
                    return responseModel;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private async Task<UserProfile> GetBspeakProfile(string accessToken)
        {
            try
            {
                var requestUrl = AppConstants.BaseUrl + "/api/UserMaster/GetCurrentUser";
                using (var client = new HttpClient())
                {
                    //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    var result = await client.GetAsync(requestUrl);
                    var responseData = await result.Content.ReadAsStringAsync();
                    var responseModel = JsonConvert.DeserializeObject<ResponseModel>(responseData);
                    var ata = Convert.ToString(responseModel.Data);
                    var profile = JsonConvert.DeserializeObject<UserProfile>(ata);
                    return profile;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}