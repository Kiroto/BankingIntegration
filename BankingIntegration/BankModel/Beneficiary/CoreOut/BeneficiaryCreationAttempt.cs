using BankingIntegration.BankModel.Beneficiary;
using BankingIntegration.BankModel.Beneficiary.In;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{
    class BeneficiaryCreationAttempt : IAuthenticated, IAttempt // Represents incoming client information requests
    {

        [JsonPropertyName("InitiatorId")]
        public int InitiatorId { get; set; }
        [JsonPropertyName("Beneficiary")]
        public BankBeneficiary Bene { get; set; }

        public string ActionName => "InsertarBeneficiario";

        public BeneficiaryCreationAttempt(BeneficiaryCreationRequest bcr, int initiatorId)
        {
            InitiatorId = initiatorId;
            Bene = bcr.Bene;
        }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
