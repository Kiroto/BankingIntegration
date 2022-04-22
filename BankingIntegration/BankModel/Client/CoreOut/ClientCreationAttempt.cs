using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{
    class ClientCreationAttempt : IAttempt, IAuthenticated // The information sent to the core on attemtping to create a client
    {
        [JsonPropertyName("InitiatorId")]
        public int InitiatorId { get; set; }
        [JsonPropertyName("Client")]
        public BankClient BankClientInfo { get; set; }

        public string ActionName => "CrearCliente";

        public ClientCreationAttempt(ClientCreationRequest ccr, int initiatorId)
        {
            InitiatorId = initiatorId;
            BankClientInfo = ccr.BankClientInfo;
        }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
