using BankingIntegration.HTTP;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace BankingIntegration
{
    class HttpReqLog : Log
    {
        public HttpMethod Method;
        public HttpReqLog(string message, HttpMethod httpMethod) : base(message)
        {
            Method = httpMethod;
        }

        public HttpReqLog(HttpListenerRequest req, int status)
        {
            Message = $"Served {req.Url.LocalPath}";
            if (status == -2)
            {
                Message += " but the path was not handled.";
                Severity = LogSeverity.Warning;
            } else if (status == -1)
            {
                Message += " but the method was not handled.";
                Severity = LogSeverity.Warning;
            } else
            {
                Message += $" handler sent status {status}";
            }
            Method = Route.MethodFromString(req.HttpMethod);
        }

        override public string ToString()
        {
            return $"[{Source}][{Route.StringFromMethod(Method)}] {Message}";
        }
    }
}
