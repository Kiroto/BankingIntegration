using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{
    class AccountCreationAttempt : BankSerializable, Authenticated // The information sent to the core on attemtping to create a client
    {
        [JsonPropertyName("InitiatorId")]
        public int InitiatorId { get; set; }
        [JsonPropertyName("Account")]
        public BankAccount BankAccountInfo { get; set; }
        [JsonPropertyName("RequestId")]
        public string RequestId { get; set; }

        public AccountCreationAttempt(AccountCreationRequest acr, int initiatorId)
        {
            InitiatorId = initiatorId;
            BankAccountInfo = acr.BankAccountInfo;
            RequestId = acr.RequestId;
        }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
