using FlashCard.Sinch;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Sinch.Core
{
	public class Security
	{
		private readonly string _applicationKey;

		private readonly string _applicationSecret;

		public Security()
		{
			throw new Exception("Must be initialized with a application key and secret");
		}

		public Security(string applicationKey, string applicationSecret)
		{
			this._applicationKey = applicationKey;
			this._applicationSecret = applicationSecret;
		}

		public string MD5Body(string body)
		{
           
            return Convert.ToBase64String(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(body)));
        }

		public string SignGetRequest(string httpMethod, string url, string timeStamp)
		{
			string[] strArrays = new string[] { httpMethod, "\n\n\nx-timestamp:", timeStamp, "\n", url };
			string tosign = string.Concat(strArrays);
			return string.Concat(this._applicationKey, ":", this.SignString(tosign, this._applicationSecret));
		}

		public string SignRequest(string httpMethod, string requestBody, string url, string timeStamp)
		{
			string[] strArrays = new string[] { httpMethod, "\n", this.MD5Body(requestBody), "\napplication/json; charset=utf-8\nx-timestamp:", timeStamp, "\n", url };
			string tosign = string.Concat(strArrays);
			return string.Concat(this._applicationKey, ":", this.SignString(tosign, this._applicationSecret));
		}

		private string SignString(string stringtoSign, string secret)
		{
			HMACSHA256 sha256 = new HMACSHA256(Convert.FromBase64String(secret));
			string signature = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(stringtoSign)));
			return signature;
		}
	}
}