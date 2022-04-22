using System;
using System.Collections.Generic;
using System.Text;

namespace BankingIntegration.BankModel
{
    interface IRequest<T> where T : BankSerializable
    {
        public T ToAttempt(int initiatorId);
    }
}
