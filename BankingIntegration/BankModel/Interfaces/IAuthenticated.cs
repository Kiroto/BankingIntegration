using System;
using System.Collections.Generic;
using System.Text;

namespace BankingIntegration.BankModel
{
    interface IAuthenticated
    {
        public int InitiatorId { get; set; } // The user ID that initiates the request
    }
}
