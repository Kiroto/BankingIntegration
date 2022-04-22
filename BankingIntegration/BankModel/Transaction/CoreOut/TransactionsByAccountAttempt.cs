using BankingIntegration.BankModel.Transaction.In;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel.Transaction.CoreOut
{
    class TransactionsByAccountAttempt : IAttempt, IAuthenticated
    {
        [JsonPropertyName("InitiatorId")]
        public int InitiatorId { get;  set; }

        [JsonPropertyName("AccountNumber")]
        public int AccountNumber { get; set; }

        public string ActionName => throw new NotImplementedException();

        public TransactionsByAccountAttempt(TransactionsByAccountRequest tbar, int initiatorId)
        {
            InitiatorId = initiatorId;
            AccountNumber = tbar.AccountNumber;
        }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
