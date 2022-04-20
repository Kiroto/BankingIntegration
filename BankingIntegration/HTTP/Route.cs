using BankingIntegration.BankModel;
using BankingIntegration.HTTP;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace BankingIntegration
{
    class Route
    {
        public string HandledPath;

        public Func<string, IResponsible> DoGet;
        public Func<string, IResponsible> DoPost;
        public Func<string, IResponsible> DoDelete;
        public Func<string, IResponsible> DoUpdate;

        public Route(string path)
        {
            HandledPath = path;
        }

        public static HttpMethod MethodFromString(string str)
        {
            HttpMethod method = HttpMethod.NULL;
            switch (str.ToLower())
            {
                case "get": { method = HttpMethod.GET; break; }
                case "post": { method = HttpMethod.POST; break; }
                case "update": { method = HttpMethod.UPDATE; break; }
                case "delete": { method = HttpMethod.DELETE; break; }
            }
            return method;
        }

        public static string StringFromMethod(HttpMethod method)
        {
            string text = "NULL";
            switch (method)
            {
                case HttpMethod.GET:
                    text = "GET";
                        break;
                case HttpMethod.POST:
                    text = "POST";
                    break;
                case HttpMethod.UPDATE:
                    text = "UPDATE";
                    break;
                case HttpMethod.DELETE:
                    text = "DELETE";
                    break;
            }
            return text;
        }

        public ProcessedResponse Handle(string req, HttpMethod method)
        {
            IResponsible status = new ProcessedResponse() { StatusCode = -1 };
            Func<string, IResponsible>? handlingFunction = null;
            switch (method)
            {
                case HttpMethod.GET:
                    handlingFunction = DoGet;
                    break;
                case HttpMethod.POST:
                    handlingFunction = DoPost;
                    break;
                case HttpMethod.UPDATE:
                    handlingFunction = DoUpdate;
                    break;
                case HttpMethod.DELETE:
                    handlingFunction = DoDelete;
                    break;
            }
            if (handlingFunction != null)
            {
                status = handlingFunction(req);
            }
            return status.buildResponse();
        }
    }
}
