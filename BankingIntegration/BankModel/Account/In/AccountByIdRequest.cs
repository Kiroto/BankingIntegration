using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{

    class AccountByIdRequest : BankSerializable, Sessioned  // Represents incoming client information requests
    {

        [JsonPropertyName("SessionToken")]
        public string SessionToken { get; set; }
        [JsonPropertyName("AccountId")]
        public int AccountId { get; set; }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
