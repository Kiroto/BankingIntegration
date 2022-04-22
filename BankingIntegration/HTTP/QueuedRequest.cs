using BankingIntegration.BankModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankingIntegration.HTTP
{
    class QueuedRequest
    {
        public HttpMethod Method;
        public IAttempt Contents;
        public string Path;
        public DateTime QueuedTime;
        public bool Tried = false;
    }
}
