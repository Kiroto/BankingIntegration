using BankingIntegration.BankModel;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace BankingIntegration.HTTP
{
    class ProcessedResponse : IResponsible
    {
        public string Contents;

        public int StatusCode { get; set; } = 200;

        public void EncodeTo(HttpListenerResponse res)
        {
            res.StatusCode = StatusCode;
            res.ContentType = "application/json";
            HttpServer.EncodeMessage(res, Contents);
        }

        public ProcessedResponse buildResponse()
        {
            return this;
        }
    }
}
