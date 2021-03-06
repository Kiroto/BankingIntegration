using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{
    class AccountByIdAttempt : IAuthenticated, IAttempt // Represents incoming client information requests
    {

        [JsonPropertyName("InitiatorId")]
        public int InitiatorId { get; set; }
        [JsonPropertyName("AccountId")]
        public int AccountId { get; set; }

        public string ActionName => throw new NotImplementedException();

        public AccountByIdAttempt(AccountByIdRequest abir, int initiatorId)
        {
            InitiatorId = initiatorId;
            AccountId = abir.AccountId;
        }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
