using BankingIntegration.HTTP;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankingIntegration.BankModel
{
    interface IResponsible
    {
        public int StatusCode { get; set; }
        public ProcessedResponse buildResponse();
    }
}
