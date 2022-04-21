using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{
    class UserLoginRequest : BankSerializable
    {
        [JsonPropertyName("Username")]
        public string Username { get; set; }

        [JsonPropertyName("PasswordHash")]
        public string PasswordHash { get; set; }

        // Which service made the request
        [JsonPropertyName("ServiceId")]
        public int ServiceId { get; set; }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
