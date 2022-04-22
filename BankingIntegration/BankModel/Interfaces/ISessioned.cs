using System;
using System.Collections.Generic;
using System.Text;

namespace BankingIntegration.BankModel
{
    interface ISessioned
    {
        public string SessionToken { get; set; }
    }
}
