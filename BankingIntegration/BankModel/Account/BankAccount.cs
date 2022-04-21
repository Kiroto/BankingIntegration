using BankingIntegration.BankModel.General;
using BankingIntegration.HTTP;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{
    class BankAccount : BankSerializable, IResponsible
    {
        [JsonPropertyName("Id")]
        public int Id { get; set; }
        [JsonPropertyName("ClientId")]
        public int ClientId { get; set; }
        [JsonPropertyName("State")]
        public int State { get; set; }
        [JsonPropertyName("AccountNumber")]
        public string AccountNumber { get; set; }
        [JsonPropertyName("Balance")]
        public float Balance { get; set; }
        [JsonPropertyName("RegisterDate")]
        public DateTime RegisterDate { get; set; }
        [JsonPropertyName("AccountType")]
        public int AccountType { get; set; }

        public int StatusCode { get; set; }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }

        public ProcessedResponse buildResponse()
        {
            return new ProcessedResponse()
            {
                Contents = AsJsonString(),
                StatusCode = StatusCode
            };
        }
    }
}
