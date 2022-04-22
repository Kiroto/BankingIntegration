using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{
    class ClientCreationAttempt : IAttempt, Authenticated // The information sent to the core on attemtping to create a client
    {
        [JsonPropertyName("InitiatorId")]
        public int InitiatorId { get; set; }
        [JsonPropertyName("Client")]
        public BankClient BankClientInfo { get; set; }
        [JsonPropertyName("RequestId")]
        public string RequestId { get; set; }

        public string ActionName => "CrearCliente";

        public ClientCreationAttempt(ClientCreationRequest ccr, int initiatorId)
        {
            InitiatorId = initiatorId;
            BankClientInfo = ccr.BankClientInfo;
            RequestId = ccr.RequestId;
        }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
