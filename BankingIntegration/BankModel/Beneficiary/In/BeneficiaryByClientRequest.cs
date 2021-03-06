using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel.Beneficiary.In
{
    class BeneficiaryByClientRequest : BankSerializable, ISessioned, IRequest<BeneficiaryByClientAttempt>
    {
        [JsonPropertyName("SessionToken")]
        public string SessionToken { get; set; }
        [JsonPropertyName("ClientId")]
        public int ClientId { get; set; }

        public BeneficiaryByClientAttempt ToAttempt(int initiatorId)
        {
            return new BeneficiaryByClientAttempt(this, initiatorId);
        }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
