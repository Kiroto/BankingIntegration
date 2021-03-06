using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{

    class AccountsByClientRequest : BankSerializable, ISessioned, IRequest<AccountsByClientAttempt>  // Represents incoming client information requests
    {

        [JsonPropertyName("SessionToken")]
        public string SessionToken { get; set; }
        [JsonPropertyName("ClientId")]
        public int ClientId { get; set; }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }

        public AccountsByClientAttempt ToAttempt(int initiatorId)
        {
            return new AccountsByClientAttempt(this, initiatorId);
        }
    }
}
