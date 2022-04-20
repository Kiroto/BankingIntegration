using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel.General
{
    class BankUser
    {
        [JsonPropertyName("Id")]
        public int Id { get; set; }
        [JsonPropertyName("Username")]
        public string Username { get; set; }
        [JsonPropertyName("Password")]
        public string Password { get; set; }
        [JsonPropertyName("LastLoginTime")]
        public DateTime LastLoginTime { get; set; }
    }
}
