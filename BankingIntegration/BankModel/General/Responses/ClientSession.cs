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

        public void FillWith(UserSession us)
        {
            UserId = us.UserId;
        }

        public ClientSession(BankClient bc)
        {
            SessionToken = sha256_hash(bc.User.Username + bc.Id + DateTime.Now);
        }
    }
}
