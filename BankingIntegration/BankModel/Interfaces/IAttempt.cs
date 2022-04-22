using System;
using System.Collections.Generic;
using System.Text;

namespace BankingIntegration.BankModel
{
    interface IAttempt : BankSerializable
    {
        public string ActionName { get; }
    }
}
