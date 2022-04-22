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

        public string ActionName => "editarCuenta";

        public AccountEditionAttempt(AccountEditionRequest aer, int initiatorId)
        {
            InitiatorId = initiatorId;
            BankAccountInfo = aer.BankAccountInfo;
        }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
