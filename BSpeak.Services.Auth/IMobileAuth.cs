using System;
using System.Threading.Tasks;
using Bspeak.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sinch.Core;
using Sinch.Verification;

namespace Bspeak.Services.Auth
{
    public interface IMobileAuth
    {
        Task<VerificationCodeResponse> RequestVerificationCode(string phoneNumber);
        Task<VerifyCodeResponse> VerifyCode(string phoneNumber, string code);
    }

    public class VerifyCodeResponse
    {
        public string Id;
        public string Message;

        public bool Success
        {
            get
            {
                if (int.TryParse(Id, out int id))
                {
                    return id > 0 && Message == "SUCCESSFUL";
                }
                return false;
            }
        }
    }

    public class VerificationCodeResponse
    {
        public string Id;
        public string Message;
        public bool HasErrors => string.IsNullOrWhiteSpace(Id);
    }

    public class MobileAuth : IMobileAuth
    {
        private readonly string _baseUrl = "https://api.sinch.com/verification/v1/verifications";

        public async Task<VerificationCodeResponse> RequestVerificationCode(string phoneNumber)
        {
            using (var client = new Client(AppConstants.SinchAppKey, AppConstants.SinchAppSecret))
            {
                var body = new VerificationRequest
                {
                    identity = new Identity
                    {
                        type = "number",
                        endpoint = phoneNumber
                    },
                    method = "sms"
                };

                var result = await client.PostAsJsonAsync(_baseUrl, body);
                var returnValue = new VerificationCodeResponse();
                if (result.IsSuccessStatusCode)
                    try
                    {
                        var response = await result.Content.ReadAsStringAsync();
                        var jsonobj = JsonConvert.DeserializeObject<JObject>(response);
                        returnValue = new VerificationCodeResponse
                        {
                            Id = jsonobj["id"].ToString()
                        };
                        return returnValue;
                    }
                    catch (Exception ex)
                    {
                        returnValue.Message = ex.Message;
                        return returnValue;
                    }
                returnValue.Message = result.ReasonPhrase;
                return returnValue;
            }
        }

        public async Task<VerifyCodeResponse> VerifyCode(string phoneNumber, string code)
        {
            using (var client = new Client(AppConstants.SinchAppKey, AppConstants.SinchAppSecret))
            {
                var request = new {method = "sms", sms = new {code}};
                var result = await client.PutAsJsonAsync(_baseUrl + "/number/" + phoneNumber, request);
                if (result.IsSuccessStatusCode)
                {
                    var responseData = await result.Content.ReadAsStringAsync();
                    var responseModel = JsonConvert.DeserializeObject<VerificationResultResponse>(responseData);
                    return new VerifyCodeResponse
                    {
                        Id = responseModel.id,
                        Message = responseModel.status
                    };
                }
                return new VerifyCodeResponse
                {
                    Id = "0",
                    Message = result.ReasonPhrase
                };
            }
        }
    }
}