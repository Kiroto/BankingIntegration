using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace BankingIntegration
{
    class Route
    {
        public string HandledPath;
        public Dictionary<string, Func<HttpListenerRequest, HttpListenerResponse, int>> HandledMethods = new Dictionary<string, Func<HttpListenerRequest, HttpListenerResponse, int>>();

        public Route(Func<HttpListenerRequest, HttpListenerResponse, int> getFunc)
        {
            HandledMethods.Add("get", getFunc);
        }
        public Route(string path)
        {
            HandledPath = path;
        }
    }
}
