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

        public Func<HttpListenerRequest, HttpListenerResponse, int> DoGet;
        public Func<HttpListenerRequest, HttpListenerResponse, int> DoPost;
        public Func<HttpListenerRequest, HttpListenerResponse, int> DoDelete;
        public Func<HttpListenerRequest, HttpListenerResponse, int> DoUpdate;

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

        public int Handle(HttpListenerRequest req, HttpListenerResponse res)
        {
            int status = -1;
            HttpMethod method = MethodFromString(req.HttpMethod);
            Func<HttpListenerRequest, HttpListenerResponse, int>? handlingFunction = null;
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
                status = handlingFunction(req, res);
            }
            return status;
        }
    }
}
