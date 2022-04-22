using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{

    class ClientCreationRequest : BankSerializable, Sessioned, IRequest<ClientCreationAttempt> // Represents incoming client creation requests
    {
        [JsonPropertyName("SessionToken")]
        public string SessionToken { get; set; }
        [JsonPropertyName("Client")]
        public BankClient BankClientInfo { get; set; }
        [JsonPropertyName("RequestId")]
        public string RequestId { get; set; }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }

        public ClientCreationAttempt ToAttempt(int initiatorId)
        {
            return new ClientCreationAttempt(this, initiatorId);
        }
    }
}
