using BankingIntegration.HTTP;
using BankingIntegration.HTTP.Exceptions;
using System;
using System.Runtime.Serialization;

namespace BankingIntegration
{
    [Serializable]
    internal class NoSuchUserSessionException : ForwardFacingException
    {
        public NoSuchUserSessionException()
        {
        }

        public NoSuchUserSessionException(string message) : base(message)
        {
        }

        public NoSuchUserSessionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoSuchUserSessionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override ProcessedResponse ToResponse()
        {
            return IntegrationServer.invalidCredentialsResponse;
        }
    }
}