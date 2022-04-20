using System;
using System.Collections.Generic;
using System.Text;

namespace BankingIntegration.BankModel
{
    interface Sessioned
    {
        public string SessionToken { get; set; }
    }
}
