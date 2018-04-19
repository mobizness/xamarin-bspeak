using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sinch.Core;

namespace Sinch.Verification
{
    public class MobileAuth
    {
        private readonly string _applicationKey;
        private readonly string _applicationSecret;
        private readonly string _baseUrl = "https://api.sinch.com/verification/v1/verifications";

        public MobileAuth()
        {
            throw new Exception("Client must be initialized with kay and secret");
        }

        public MobileAuth(string applicationKey, string applicationSecret)
        {
            _applicationKey = applicationKey;
            _applicationSecret = applicationSecret;
        }

        /// <summary>
        ///     Use this method to verify a code a user enters, if result is 0 check status return for reason
        /// </summary>
        /// <param name="phoneNumber">normailized number</param>
        /// <param name="pincode">code the user enters</param>
        /// <returns></returns>
        public async Task<VerificationResultResponse> VerifySMSCode(string phoneNumber, string pincode)
        {
            using (var client = new Client(_applicationKey, _applicationSecret))
            {
                var request = new {method = "sms", sms = new {code = pincode}};
                var result = await client.PutAsJsonAsync(_baseUrl + "/number/" + phoneNumber, request);
                if (result.IsSuccessStatusCode)
                {
                    var sr = new StreamReader(await result.Content.ReadAsStreamAsync());
                    var responseData = sr.ReadToEnd().Trim();
                    var ResponseModel = JsonConvert.DeserializeObject<VerificationResultResponse>(responseData);
                    return ResponseModel;
                }
                return new VerificationResultResponse
                {
                    id = "0",
                    method = "sms",
                    status = result.ReasonPhrase
                };
            }
            return null;
        }

        /// <summary>
        ///     Initiate a SMS verification process,
        /// </summary>
        /// <param name="phoneNumber">e164 phonnumber</param>
        /// <returns></returns>
        public async Task<VerificationResponse> StartSMSVerification(string phoneNumber)
        {
            using (var client = new Client(_applicationKey, _applicationSecret))
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
                var returnValue = new VerificationResponse();
                if (result.IsSuccessStatusCode)
                    try
                    {
                        var response = await result.Content.ReadAsStringAsync();
                        var jsonobj = JsonConvert.DeserializeObject<JObject>(response);
                        returnValue = new VerificationResponse
                        {
                            Id = jsonobj["id"].ToString(),
                            SmsTemplate = jsonobj["sms"]["template"].ToString()
                        };
                        return returnValue;
                    }
                    catch (Exception ex)
                    {
                        returnValue.StatusMessage = ex.Message;
                        return returnValue;
                    }
                returnValue.StatusMessage = result.ReasonPhrase;
                return returnValue;
            }
        }
    }

    /// <summary>
    ///     REsponse class for verification of a code
    /// </summary>
    public class VerificationResultResponse
    {
        /// <summary>
        ///     will return 0 if errors, check status for reason
        /// </summary>
        public string id { get; set; }

        public string method { get; set; }

        /// <summary>
        ///     successful if ok, id should be higher than 0
        /// </summary>
        public string status { get; set; }

        public bool intercepted { get; set; }
    }

    internal class VerificationRequest
    {
        public Identity identity { get; set; }
        public string method { get; set; }
    }

    internal class Identity
    {
        public string type { get; set; }
        public string endpoint { get; set; }
    }

    public class VerificationResponse
    {
        /// <summary>
        ///     The id of the verification request, this is the id you should send with a query to request status of a verification
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     The format of the message, could be used in android to parse sms automaticly
        /// </summary>
        public string SmsTemplate { get; set; }

        /// <summary>
        ///     If an error occurs message will be here.
        /// </summary>
        public string StatusMessage { get; set; }
    }
}