using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http; 
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
namespace Sinch.SMS {
    public class Client {
        private string _applicationKey;
        private string _applicationSecret;
        private string baseURL = "https://messagingapi.sinch.com/v1/sms/";
        public Client() {
            throw new Exception("Client must be initialized with kay and secret");
        }
        public Client(string applicationKey, string applicationSecret) {
            _applicationKey = applicationKey;
            _applicationSecret = applicationSecret;
        }

        public async Task<int> SendSMS(string number, string message) {
            try
            {
                using (var httpClient = new Sinch.Core.Client(_applicationKey, _applicationSecret)) {
                    var url = baseURL + number;
                    var smsRequest = new SMSRequest { Message = message };
                    var response = await httpClient.PostAsJsonAsync(url, smsRequest);
                    System.IO.StreamReader sr = new System.IO.StreamReader(await response.Content.ReadAsStreamAsync());
                  var  responseData = sr.ReadToEnd().Trim();
                    response.EnsureSuccessStatusCode();
                    //var result = await response.Content.ReadAsAsync<SMSResult>();
                    // return result.MessageId;
                    return 2;
                }
            }
            catch (Exception ex)
            {
                string str = ex.Message;
                string at = str;
                return 0;
            }

            
        }

        //public async Task<int> SendSMS(string fromNumber, string number, string message) {
        //    var url = baseURL + number;
        //    var smsRequest = new SMSRequest { Message = message, From = fromNumber };
        //    string errormessage = "";
        //    using (var httpClient = new Sinch.Core.Client(_applicationKey, _applicationSecret)) {
        //        try {
        //            var response = await httpClient.PostAsJsonAsync(url, smsRequest);
        //            errormessage = response.ReasonPhrase;
        //            response.EnsureSuccessStatusCode();
        //            var result = await response.Content.ReadAsAsync<SMSResult>();
        //            return result.MessageId;
        //        } catch (Exception ex) {

        //            throw new Exception(errormessage);

        //        }

        //    }
        //}

        //public async Task<SMSStatus> CheckStatus(int messageid) {
        //    var url = baseURL + messageid;

        //    using (var httpClient = new Sinch.Core.Client(_applicationKey, _applicationSecret)) {

        //        var response = await httpClient.GetAsync(url);
        //        response.EnsureSuccessStatusCode();
        //        var result = await response.Content.ReadAsAsync<SMSStatusResult>();
        //        return result.Status;
        //    }
        //}
    }

    public enum SMSStatus {
        Unknown = 1,
        Pending = 2,
        Successful = 3,
        Faulted = 4
    }

    //private sealed class SMSStatus1
    //{

    //    private readonly String name;
    //    private readonly int value;

    //    public static readonly SMSStatus1 Unknown  = new SMSStatus1(1, "Unknown");
    //    public static readonly SMSStatus1 Pending = new SMSStatus1(2, "Pending");
    //    public static readonly SMSStatus1 Successful = new SMSStatus1(3, "Successful");
    //    public static readonly SMSStatus1 Faulted = new SMSStatus1(3, "Faulted");


    //    private SMSStatus1(int value, String name)
    //    {
    //        this.name = name;
    //        this.value = value;
    //    }

    //    public override String ToString()
    //    {
    //        return name;
    //    }

    //}
    public class SMSStatusResult {

        public SMSStatus Status { get; set; }
    }
}
