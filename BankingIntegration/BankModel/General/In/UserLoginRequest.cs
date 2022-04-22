using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{
    class UserLoginRequest : BankSerializable, IRequest<UserLoginRequest>, IAttempt
    {
        [JsonPropertyName("Username")]
        public string Username { get; set; }

        [JsonPropertyName("PasswordHash")]
        public string PasswordHash { get; set; }

        // Which service made the request
        [JsonPropertyName("ServiceId")]
        public int ServiceId { get; set; }

        public string ActionName => "Login";

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }

        public UserLoginRequest ToAttempt(int initiatorId)
        {
            return this;
        }
    }
}
