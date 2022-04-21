using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{

    class AccountDeletionRequest : BankSerializable, Sessioned, IAttemptable<AccountDeletionAttempt>  // Represents incoming client deletion requests
    {

        [JsonPropertyName("SessionToken")]
        public string SessionToken { get; set; }
        [JsonPropertyName("AccountId")]
        public int AccountId { get; set; }
        [JsonPropertyName("RequestId")]
        public string RequestId { get; set; }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }

        public AccountDeletionAttempt ToAttempt(int initiatorId)
        {
            return new AccountDeletionAttempt(this, initiatorId);
        }
    }
}
