using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{

    class ClientInfoRequest : BankSerializable, Sessioned, IRequest<ClientInfoAttempt>  // Represents incoming client information requests
    {

        [JsonPropertyName("SessionToken")]
        public string SessionToken { get; set; }
        [JsonPropertyName("ClientId")]
        public int ClientId { get; set; }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }

        public ClientInfoAttempt ToAttempt(int initiatorId)
        {
            return new ClientInfoAttempt(this, initiatorId);
        }
    }
}
