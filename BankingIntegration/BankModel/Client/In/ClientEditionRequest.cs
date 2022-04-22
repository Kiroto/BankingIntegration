using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{

    class ClientEditionRequest : BankSerializable, Sessioned, IRequest<ClientEditionAttempt>  // Represents incoming client edition requests
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

        public ClientEditionAttempt ToAttempt(int initiatorId)
        {
            return new ClientEditionAttempt(this, initiatorId);
        }
    }
}
