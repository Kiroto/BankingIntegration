using BankingIntegration.BankModel.Transaction.CoreOut;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel.Transaction.In
{
    class TransactionRequest : BankSerializable, ISessioned, IRequest<TransactionAttempt>
    {
        [JsonPropertyName("SessionToken")]
        public string SessionToken { get; set; }

        [JsonPropertyName("Transaction")]
        public Transaction Tran { get; set; }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }

        public TransactionAttempt ToAttempt(int initiatorId)
        {
            return new TransactionAttempt(this, initiatorId);
        }
    }
}
