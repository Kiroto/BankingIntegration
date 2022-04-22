using BankingIntegration.HTTP;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel.Transaction
{
    class Transaction : BankSerializable, IResponsible
    {
        [JsonPropertyName("Id")]
        public int TransactionId { get; set; }

        [JsonPropertyName("SourceAccountId")]
        public int SourceAccountId { get; set; }

        [JsonPropertyName("TargetAccountId")]
        public int TargetAccountId { get; set; }

        [JsonPropertyName("TransactionType")] // 1 es Deposito, 2 retiro, 3 cuentas tercero
        public int TransactionType { get; set; }

        [JsonPropertyName("Amount")]
        public float Amount { get; set; }
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
