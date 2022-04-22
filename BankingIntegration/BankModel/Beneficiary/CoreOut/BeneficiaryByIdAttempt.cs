using BankingIntegration.BankModel.Beneficiary.In;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{
    class BeneficiaryByIdAttempt : IAuthenticated, IAttempt // Represents incoming client information requests
    {

        [JsonPropertyName("InitiatorId")]
        public int InitiatorId { get; set; }
        [JsonPropertyName("BeneficiaryId")]
        public int BeneficiaryId { get; set; }

        public string ActionName => throw new NotImplementedException();

        public BeneficiaryByIdAttempt(BeneficiaryByIdRequest bbir, int initiatorId)
        {
            InitiatorId = initiatorId;
            BeneficiaryId = bbir.BeneficiaryId;
        }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
