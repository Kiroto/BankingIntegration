using System;
using System.Collections.Generic;
using System.Text;

namespace BankingIntegration
{
    class HttpReqLog : Log
    {
        public string HttpMethod;
        public HttpReqLog(string message, string httpMethod) : base(message)
        {
            HttpMethod = httpMethod;
        }

        override public string ToString()
        {
            return $"[{Source}][{HttpMethod.ToUpper()}] {Message}";
        }
    }
}
