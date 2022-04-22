using BankingIntegration.HTTP;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{
    class ClientSession : UserSession
    {
        [JsonPropertyName("ClientId")]
        public int ClientId { get; set; }

        public ClientSession(BankClient bc)
        {
            ClientId = bc.Id;   
            UserId = bc.User.Id;
            SessionToken = sha256_hash(bc.User.Username + bc.Id + DateTime.Now);
        }
    }
}
