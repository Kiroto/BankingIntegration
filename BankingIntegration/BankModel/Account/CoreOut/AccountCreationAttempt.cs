using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{
    class AccountCreationAttempt : IAttempt, IAuthenticated // The information sent to the core on attemtping to create a client
    {
        [JsonPropertyName("InitiatorId")]
        public int InitiatorId { get; set; }
        [JsonPropertyName("Account")]
        public BankAccount BankAccountInfo { get; set; }

        public string ActionName => "CrearCuenta";

        public AccountCreationAttempt(AccountCreationRequest acr, int initiatorId)
        {
            InitiatorId = initiatorId;
            BankAccountInfo = acr.BankAccountInfo;
        }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
