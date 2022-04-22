using BankingIntegration.BankModel.General;
using BankingIntegration.HTTP;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel.Employee
{
    class BankEmployee : BankSerializable, IResponsible
    {
        [JsonPropertyName("Id")]
        public int Id { get; set; }
        [JsonPropertyName("User")]
        public BankUser User { get; set; }
        [JsonPropertyName("SucursalId")]
        public int SucursalId { get; set; }
        [JsonPropertyName("Nivel")]
        public int Nivel { get; set; }

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
