using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{

    class ClientDeletionRequest : BankSerializable, ISessioned, IRequest<ClientDeletionAttempt>  // Represents incoming client deletion requests
    {

        [JsonPropertyName("SessionToken")]
        public string SessionToken { get; set; }
        [JsonPropertyName("ClientId")]
        public int ClientId { get; set; }
        [JsonPropertyName("RequestId")]
        public string RequestId { get; set; }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }

        public ClientDeletionAttempt ToAttempt(int initiatorId)
        {
            return new ClientDeletionAttempt(this, initiatorId);
        }
    }
}
