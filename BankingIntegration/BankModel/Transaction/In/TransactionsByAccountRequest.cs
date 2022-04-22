using BankingIntegration.BankModel.Transaction.CoreOut;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel.Transaction.In
{
    class TransactionsByAccountRequest : BankSerializable, Sessioned, IRequest<TransactionsByAccountAttempt>
    {
        [JsonPropertyName("SessionToken")]
        public string SessionToken { get; set; }

        [JsonPropertyName("AccountNumber")]
        public int AccountNumber { get; set; }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }

        public TransactionsByAccountAttempt ToAttempt(int initiatorId)
        {
            return new TransactionsByAccountAttempt(this, initiatorId);
        }
    }
}
