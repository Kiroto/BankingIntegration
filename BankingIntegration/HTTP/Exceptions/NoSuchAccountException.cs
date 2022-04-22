using BankingIntegration.HTTP;
using BankingIntegration.HTTP.Exceptions;
using System;
using System.Runtime.Serialization;

namespace BankingIntegration
{
    [Serializable]
    internal class NoSuchAccountException : ForwardFacingException
    {
        public NoSuchAccountException()
        {
        }

        public NoSuchAccountException(string message) : base(message)
        {
        }

        public NoSuchAccountException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoSuchAccountException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override ProcessedResponse ToResponse()
        {
            return IntegrationServer.invalidCredentialsResponse;
        }
    }
}