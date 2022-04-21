using System;
using System.Collections.Generic;
using System.Text;

namespace BankingIntegration.BankModel
{
    interface IAttemptable<T> where T : BankSerializable
    {
        public T ToAttempt(int initiatorId);
    }
}
