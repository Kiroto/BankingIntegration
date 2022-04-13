using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{
    class TransactionAttempt : BankSerializable
    {
        [JsonPropertyName("username")]
        public string Username { get; set; }
        [JsonPropertyName("password")]
        public string PasswordHash { get; set; }
        [JsonPropertyName("sourceAccount")]
        public int SourceAccountID { get; set; }
        [JsonPropertyName("targetAccount")]
        public int TargetAccountID { get; set; }
        [JsonPropertyName("amount")]
        public float Amount { get; set; }

        public static TransactionAttempt FromJsonString(string json)
        {
            return JsonSerializer.Deserialize<TransactionAttempt>(json);
        }

        public string asJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
