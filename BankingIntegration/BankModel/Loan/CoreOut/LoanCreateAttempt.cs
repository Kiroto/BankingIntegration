using BankingIntegration.BankModel.Beneficiary.In;
using BankingIntegration.BankModel.Loan;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{
    class LoanCreateAttempt : IAuthenticated, IAttempt // Represents incoming client information requests
    {

        [JsonPropertyName("InitiatorId")]
        public int InitiatorId { get; set; }
        [JsonPropertyName("Loan")]
        public BankLoan Loan { get; set; }

        public string ActionName => throw new NotImplementedException();

        public LoanCreateAttempt(LoanCreateRequest bbcr, int initiatorId)
        {
            InitiatorId = initiatorId;
            Loan = bbcr.Loan;
        }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
