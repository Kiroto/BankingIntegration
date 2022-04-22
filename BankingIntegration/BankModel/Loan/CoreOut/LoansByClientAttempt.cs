using BankingIntegration.BankModel.Beneficiary.In;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{
    class LoansByClientAttempt : IAuthenticated, IAttempt // Represents incoming client information requests
    {

        [JsonPropertyName("InitiatorId")]
        public int InitiatorId { get; set; }
        [JsonPropertyName("ClientId")]
        public int ClientId { get; set; }

        public string ActionName => "BuscarPrestamosPorCliente";

        public LoansByClientAttempt(LoansByClientRequest bbcr, int initiatorId)
        {
            InitiatorId = initiatorId;
            ClientId = bbcr.ClientId;
        }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
