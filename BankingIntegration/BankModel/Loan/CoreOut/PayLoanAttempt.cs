using BankingIntegration.BankModel.Beneficiary.In;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{
    class PayLoanAttempt : IAuthenticated, IAttempt // Represents incoming client information requests
    {

        [JsonPropertyName("InitiatorId")]
        public int InitiatorId { get; set; }
        [JsonPropertyName("LoanId")]
        public int LoanId { get; set; }
        [JsonPropertyName("SourceAccountId")]
        public int SourceAccountId { get; set; }
        [JsonPropertyName("PayAmount")]
        public float PayAmount { get; set; }

        public string ActionName => throw new NotImplementedException();

        public PayLoanAttempt(PayLoanRequest plr, int initiatorId)
        {
            InitiatorId = initiatorId;
            LoanId = plr.LoanId;
            SourceAccountId = plr.SourceAccountId;
            PayAmount = plr.PayAmount;
        }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
