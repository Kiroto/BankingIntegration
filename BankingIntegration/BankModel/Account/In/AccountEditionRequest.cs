using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{

    class AccountEditionRequest : BankSerializable, ISessioned, IRequest<AccountEditionAttempt>  // Represents incoming client edition requests
    {
        [JsonPropertyName("SessionToken")]
        public string SessionToken { get; set; }
        [JsonPropertyName("Account")]
        public BankAccount BankAccountInfo { get; set; }
        [JsonPropertyName("RequestId")]
        public string RequestId { get; set; }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }

        public AccountEditionAttempt ToAttempt(int initiatorId)
        {
            return new AccountEditionAttempt(this, initiatorId);
        }
    }
}
