using BankingIntegration.HTTP;
using BankingIntegration.HTTP.Exceptions;
using System;
using System.Runtime.Serialization;

namespace BankingIntegration
{
    [Serializable]
    internal class CoreEndpointNotReached : ForwardFacingException
    {
        public CoreEndpointNotReached()
        {
        }

        public CoreEndpointNotReached(string message) : base(message)
        {
        }

        public CoreEndpointNotReached(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CoreEndpointNotReached(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override ProcessedResponse ToResponse()
        {
            return IntegrationServer.coreEndpointNotReachedResponse;
        }
    }
}