using BankingIntegration.HTTP;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace BankingIntegration.BankModel.General.Responses
{
    class BList<A> : List<A>, BankSerializable, IResponsible
    {
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
