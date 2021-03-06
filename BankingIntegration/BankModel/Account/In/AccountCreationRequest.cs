using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{

    class AccountCreationRequest : BankSerializable, ISessioned, IRequest<AccountCreationAttempt> // Represents incoming client creation requests
    {
        [JsonPropertyName("SessionToken")]
        public string SessionToken { get; set; }
        [JsonPropertyName("Account")] // Client ID is inside the account
        public BankAccount BankAccountInfo { get; set; }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }

        public AccountCreationAttempt ToAttempt(int initiatorId)
        {
            return new AccountCreationAttempt(this, initiatorId);
        }
    }
}
