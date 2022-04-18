﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingIntegration.BankModel
{
    class ClientCreationAttempt : BankSerializable
    {
        [JsonPropertyName("UserId")]
        public int UserId { get; set; }
        [JsonPropertyName("ClientName")]
        public string ClientName { get; set; }
        [JsonPropertyName("ClientCedula")]
        public string ClientCedula { get; set; }
        [JsonPropertyName("ClientUsername")]
        public string ClientUsername { get; set; }
        [JsonPropertyName("ClientPassword")]
        public string ClientPassword { get; set; }
        [JsonPropertyName("Sex")]
        public int Sex { get; set; }
        [JsonPropertyName("RequestId")]
        public string RequestId { get; set; }

        public ClientCreationAttempt(ClientCreationRequest ccr, int userId)
        {
            UserId = userId;
            ClientName = ccr.ClientName;
            ClientCedula = ccr.ClientCedula;
            ClientUsername = ccr.ClientUsername;
            ClientPassword = ccr.ClientPassword;
            Sex = ccr.Sex;
            RequestId = ccr.RequestId;
        }

        public string AsJsonString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
