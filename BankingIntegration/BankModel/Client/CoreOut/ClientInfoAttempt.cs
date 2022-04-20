using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{
    class ClientInfoAttempt : BankSerializable, Authenticated // Represents incoming client information requests
    {

        [JsonPropertyName("InitiatorId")]
        public int InitiatorId { get; set; }
        [JsonPropertyName("ClientId")]
        public int ClientId { get; set; }

        public ClientInfoAttempt(ClientInfoRequest cir, int initiatorId)
        {
            InitiatorId = initiatorId;
            ClientId = cir.ClientId;
        }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
