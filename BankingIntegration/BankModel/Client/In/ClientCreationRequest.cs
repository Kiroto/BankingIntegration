using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{

    class ClientCreationRequest : BankSerializable // Represents incoming client creation requests
    {
        [JsonPropertyName("EmployeeSessionToken")]
        public string EmployeeSessionToken { get; set; }
        [JsonPropertyName("ClientName")]
        public string ClientName { get; set; }
        [JsonPropertyName("ClientCedula")]
        public string ClientCedula { get; set; }
        [JsonPropertyName("ClientUsername")]
        public string ClientUsername { get; set; }
        [JsonPropertyName("ClientPassword")]
        public string ClientPassword { get; set; }
        [JsonPropertyName("Sex")]
        public int Sex { get; set; }
        [JsonPropertyName("RequestId")]
        public string RequestId { get; set; }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
