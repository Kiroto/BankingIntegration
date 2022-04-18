using System;
using System.Collections.Generic;
using System.Text;

namespace BankingIntegration.HTTP
{
    class QueuedRequest
    {
        public HttpMethod Method;
        public String Contents;
        public string Path;
        public DateTime QueuedTime;
    }
}
