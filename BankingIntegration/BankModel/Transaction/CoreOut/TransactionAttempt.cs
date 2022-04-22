using BankingIntegration.BankModel.Transaction.In;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel.Transaction.CoreOut
{
    class TransactionAttempt : IAttempt, IAuthenticated
    {
        [JsonPropertyName("InitiatorId")]
        public int InitiatorId { get;  set; }

        [JsonPropertyName("Transaction")]
        public Transaction Tran { get; set; }

        public string ActionName { get; set; } = "InsertarTransaccion";

        public TransactionAttempt(TransactionRequest tr, int initiatorId)
        {
            InitiatorId = initiatorId;
            Tran = tr.Tran;
        }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
