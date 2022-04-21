using BankingIntegration.HTTP;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{
    class UserSession : BankSerializable, IResponsible
    {
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
