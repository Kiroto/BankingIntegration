using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{
    class AccountDeletionAttempt : BankSerializable, Authenticated // Represents incoming client deletion requests
    {
        [JsonPropertyName("InitiatorId")]
        public int InitiatorId { get; set; }
        [JsonPropertyName("AccountId")]
        public int AccountId { get; set; }
        [JsonPropertyName("RequestId")]
        public string RequestId { get; set; }

        public AccountDeletionAttempt(AccountDeletionRequest adr, int initiatorId)
        {
            InitiatorId = initiatorId;
            AccountId = adr.AccountId;
            RequestId = adr.RequestId;
        }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
