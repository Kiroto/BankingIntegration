using BankingIntegration.BankModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankingIntegration.HTTP
{
    class ProcessedResponse : IResponsible
    {
        public string Contents;

        public int StatusCode { get; set; }

        public ProcessedResponse buildResponse()
        {
            return this;
        }
    }
}
