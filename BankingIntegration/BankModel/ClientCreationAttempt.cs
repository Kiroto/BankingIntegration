using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{

    class ClientCreationAttempt : BankSerializable
    {
        [JsonPropertyName("idEmpleado")]
        public int IdEmpleado { get; set; }

        public string asJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
