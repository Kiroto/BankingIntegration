using BankingIntegration.HTTP;
using BankingIntegration.HTTP.Exceptions;
using System;
using System.Runtime.Serialization;

namespace BankingIntegration
{
    [Serializable]
    internal class CoreTimeoutException : ForwardFacingException
    {
        public CoreTimeoutException()
        {
        }

        public CoreTimeoutException(string message) : base(message)
        {
        }

        public CoreTimeoutException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CoreTimeoutException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override ProcessedResponse ToResponse()
        {
            return IntegrationServer.coreOfflineResponse;
        }
    }
}