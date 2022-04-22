using BankingIntegration.HTTP;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel.Beneficiary
{
    class BankBeneficiary : BankSerializable, IResponsible
    {
        [JsonPropertyName("Id")]
        public int Id { get; set; }
        [JsonPropertyName("ClientId")]
        public int ClientId { get; set; }
        [JsonPropertyName("BeneficiaryAccountNumber")] // La cuenta del beneficiario reistrado
        public int BeneficiaryAccountNumber { get; set; }
        [JsonPropertyName("Alias")]
        public string Alias { get; set; }
        [JsonPropertyName("RegisterDate")]
        public DateTime RegisterDate { get; set; }

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
