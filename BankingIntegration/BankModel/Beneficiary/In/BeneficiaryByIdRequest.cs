using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel.Beneficiary.In
{
    class BeneficiaryByIdRequest : BankSerializable, ISessioned, IRequest<BeneficiaryByIdAttempt>
    {
        [JsonPropertyName("SessionToken")]
        public string SessionToken { get; set; }
        [JsonPropertyName("BeneficiaryId")]
        public int BeneficiaryId { get; set; }

        public BeneficiaryByIdAttempt ToAttempt(int initiatorId)
        {
            return new BeneficiaryByIdAttempt(this, initiatorId);
        }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
