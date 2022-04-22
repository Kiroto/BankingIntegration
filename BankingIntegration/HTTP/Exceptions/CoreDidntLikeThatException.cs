using BankingIntegration.HTTP;
using BankingIntegration.HTTP.Exceptions;
using System;
using System.Runtime.Serialization;

namespace BankingIntegration
{
    [Serializable]
    internal class CoreDidntLikeThatException : ForwardFacingException
    {
        public CoreDidntLikeThatException()
        {
        }

        public CoreDidntLikeThatException(string message) : base(message)
        {
        }

        public CoreDidntLikeThatException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CoreDidntLikeThatException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override ProcessedResponse ToResponse()
        {
            return IntegrationServer.coreEndpointNotReachedResponse;
        }
    }
}