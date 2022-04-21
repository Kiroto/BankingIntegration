using BankingIntegration.BankModel.General;
using BankingIntegration.HTTP;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{
    class BankClient : BankSerializable, IResponsible
    {
        [JsonPropertyName("Id")]
        public int Id { get; set; }
        [JsonPropertyName("User")]
        public BankUser User { get; set; } = new BankUser();
        [JsonPropertyName("Name")]
        public string Name { get; set; }
        [JsonPropertyName("Cedula")]
        public string Cedula { get; set; }
        [JsonPropertyName("Sex")]
        public int Sex { get; set; }
        [JsonPropertyName("RegisterDate")]
        public DateTime RegisterDate { get; set; }

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
