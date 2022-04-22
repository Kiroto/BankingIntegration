using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel.Beneficiary.In
{
    class LoansByClientRequest : BankSerializable, ISessioned, IRequest<LoansByClientAttempt>
    {
        [JsonPropertyName("SessionToken")]
        public string SessionToken { get; set; }
        [JsonPropertyName("ClientId")]
        public int ClientId { get; set; }

        public LoansByClientAttempt ToAttempt(int initiatorId)
        {
            return new LoansByClientAttempt(this, initiatorId);
        }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
