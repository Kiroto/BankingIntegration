using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel.Beneficiary.In
{
    class PayLoanRequest : BankSerializable, ISessioned, IRequest<PayLoanAttempt>
    {
        [JsonPropertyName("SessionToken")]
        public string SessionToken { get; set; }
        [JsonPropertyName("LoanId")]
        public int LoanId { get; set; }
        [JsonPropertyName("SourceAccountId")]
        public int SourceAccountId { get; set; }
        [JsonPropertyName("PayAmount")]
        public float PayAmount { get; set; }

        public PayLoanAttempt ToAttempt(int initiatorId)
        {
            return new PayLoanAttempt(this, initiatorId);
        }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
