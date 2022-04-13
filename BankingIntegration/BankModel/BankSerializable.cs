using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace BankingIntegration.BankModel
{
    interface BankSerializable
    {
        public string asJsonString();
    }
}
