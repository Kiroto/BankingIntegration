using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{
    public enum ErrorCode : ushort
    {
        CORE_OFFLINE = 1,
        CREDENTIALS_INVALID = 2,
        KEY_INVALID = 3,
    }
    class ErrorMesage : BankSerializable
    {   
        [JsonPropertyName("ErrorCode")]
        public ErrorCode Code { get; set; }

        [JsonPropertyName("ErrorMessage")]
        public string ErrorMessage { get; set; }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
