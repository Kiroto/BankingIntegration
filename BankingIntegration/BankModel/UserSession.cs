using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{
    class UserSession : BankSerializable
    {
        // The user ID
        [JsonPropertyName("UserId")]
        public int UserID { get; set; }

        // The session token for the user.
        [JsonPropertyName("SessionToken")]
        public string SessionToken { get; set; }

        // When this token last made its request
        public DateTime LastRequest { get; set; }
        
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
    }
}
