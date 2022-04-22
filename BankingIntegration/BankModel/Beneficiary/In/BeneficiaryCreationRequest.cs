using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel.Beneficiary.In
{
    class BeneficiaryCreationRequest : BankSerializable, ISessioned, IRequest<BeneficiaryCreationAttempt>
    { 
        [JsonPropertyName("SessionToken")]
        public string SessionToken { get; set; }
        [JsonPropertyName("Beneficiary")]
        public BankBeneficiary Bene { get; set; }

        public BeneficiaryCreationAttempt ToAttempt(int initiatorId)
        {
            return new BeneficiaryCreationAttempt(this, initiatorId);
        }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
