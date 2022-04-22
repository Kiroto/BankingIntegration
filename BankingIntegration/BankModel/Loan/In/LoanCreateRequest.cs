using BankingIntegration.BankModel.Loan;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel.Beneficiary.In
{
    class LoanCreateRequest : BankSerializable, ISessioned, IRequest<LoanCreateAttempt>
    {
        [JsonPropertyName("SessionToken")]
        public string SessionToken { get; set; }
        [JsonPropertyName("Loan")]
        public BankLoan Loan { get; set; }

        public LoanCreateAttempt ToAttempt(int initiatorId)
        {
            return new LoanCreateAttempt(this, initiatorId);
        }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
