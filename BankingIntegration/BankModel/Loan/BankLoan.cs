using BankingIntegration.HTTP;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel.Loan
{
    class BankLoan : BankSerializable, IResponsible
    {
        [JsonPropertyName("Id")]
        public int LoanId { get; set; }

        [JsonPropertyName("SourceAccountId")] // Account from which the loan comes
        public int SourceAccountId { get; set; }

        [JsonPropertyName("ReceivingClientId")] // The client that owes the loan
        public int ReceivingClientId { get; set; }

        [JsonPropertyName("TotalLoanAmount")] // The total Amount owed
        public float TotalLoanAmount { get; set; }

        [JsonPropertyName("TotalPaidAmount")] // The total Amount paid
        public float TotalPaidAmount { get; set; }

        [JsonPropertyName("CreationDate")]
        public DateTime CreationDate { get; set; } = DateTime.Now;

        [JsonPropertyName("State")] // The state of the loan
        public int State { get; set; }

        [JsonPropertyName("Rate")] // The total Amount paid
        public float Rate { get; set; }

        public int StatusCode { get; set; }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }

        public ProcessedResponse buildResponse()
        {
            return new ProcessedResponse()
            {
                Contents = AsJsonString(),
                StatusCode = StatusCode
            };
        }
    }
}
