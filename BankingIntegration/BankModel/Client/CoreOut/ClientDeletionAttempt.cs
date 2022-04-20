using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{
    class ClientDeletionAttempt : BankSerializable, Authenticated // Represents incoming client deletion requests
    {
        [JsonPropertyName("InitiatorId")]
        public int InitiatorId { get; set; }
        [JsonPropertyName("ClientId")]
        public int ClientId { get; set; }
        [JsonPropertyName("RequestId")]
        public string RequestId { get; set; }

        public ClientDeletionAttempt(ClientDeletionRequest cdr, int initiatorId)
        {
            InitiatorId = initiatorId;
            ClientId = cdr.ClientId;
            RequestId = cdr.RequestId;
        }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
