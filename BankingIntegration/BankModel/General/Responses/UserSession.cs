using BankingIntegration.HTTP;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{
    class UserSession : BankSerializable, IResponsible
    {
        public static string sha256_hash(string value)
        {
            StringBuilder Sb = new StringBuilder();

            using (var hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }

        // The user ID
        [JsonPropertyName("UserId")]
        public int UserId { get; set; }

        // The session token for the user.
        [JsonPropertyName("SessionToken")]
        public string SessionToken { get; set; }

        // When this token last made its request
        public DateTime LastRequest;
        
        // When the session started
        public DateTime SessionStart;

        // The requesting service
        public int Service;

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }

        public void Refresh()
        {
            LastRequest = DateTime.Now;
        }

        public void Initialize()
        {
            DateTime currentTime = DateTime.Now;
            SessionStart = currentTime;
            LastRequest = currentTime;            
        }

        public int StatusCode { get; set; }

        public ProcessedResponse buildResponse()
        {
            return new ProcessedResponse() {
                Contents = AsJsonString(),
                StatusCode = StatusCode
            };
        }
    }
}
