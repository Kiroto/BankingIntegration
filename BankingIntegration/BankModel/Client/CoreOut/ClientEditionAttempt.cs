using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{
    class ClientEditionAttempt : BankSerializable, Authenticated // Represents outgoing client edition attempts
    {
        [JsonPropertyName("InitiatorId")]
        public int InitiatorId { get; set; }
        [JsonPropertyName("Client")]
        public BankClient BankClientInfo { get; set; }
        [JsonPropertyName("RequestId")]
        public string RequestId { get; set; }

        public ClientEditionAttempt(ClientEditionRequest cer, int initiatorId)
        {
            InitiatorId = initiatorId;
            BankClientInfo = cer.BankClientInfo;
            RequestId = cer.RequestId;
        }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
