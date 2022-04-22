using BankingIntegration.HTTP;
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
        CORE_ERROR = 4,
    }
    class ErrorMesage : BankSerializable, IResponsible
    {   
        [JsonPropertyName("ErrorCode")]
        public ErrorCode Code { get; set; }

        [JsonPropertyName("ErrorMessage")]
        public string ErrorMessage { get; set; }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }

        public int StatusCode { get; set; }

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
