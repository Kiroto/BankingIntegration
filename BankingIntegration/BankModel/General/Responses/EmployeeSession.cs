using BankingIntegration.BankModel.Employee;
using BankingIntegration.BankModel.General;
using BankingIntegration.HTTP;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{
    class EmployeeSession : UserSession
    {
        [JsonPropertyName("EmployeeId")]
        public int EmployeeId { get; set; }

        public void FillWith(UserSession us)
        {
            UserId = us.UserId;
        }

        public EmployeeSession(BankEmployee be)
        {
            UserId = be.User.Id;
            EmployeeId = be.Id;
            SessionToken = sha256_hash(be.User.Username + be.Id + DateTime.Now);
        }
    }
}
