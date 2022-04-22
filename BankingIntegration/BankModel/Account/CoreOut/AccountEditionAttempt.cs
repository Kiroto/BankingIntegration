using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{
    class AccountEditionAttempt : IAttempt, IAuthenticated // Represents outgoing client edition attempts
    {
        [JsonPropertyName("InitiatorId")]
        public int InitiatorId { get; set; }
        [JsonPropertyName("Account")]
        public BankAccount BankAccountInfo { get; set; }
        [JsonPropertyName("RequestId")]
        public string RequestId { get; set; }

        public string ActionName => throw new NotImplementedException();

        public AccountEditionAttempt(AccountEditionRequest aer, int initiatorId)
        {
            InitiatorId = initiatorId;
            BankAccountInfo = aer.BankAccountInfo;
            RequestId = aer.RequestId;
        }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
