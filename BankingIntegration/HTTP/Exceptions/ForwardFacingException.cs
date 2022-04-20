using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace BankingIntegration.HTTP.Exceptions
{
    [Serializable]
    abstract class ForwardFacingException : Exception
    {
        public ForwardFacingException()
        {
        }

        public ForwardFacingException(string message) : base(message)
        {
        }

        public ForwardFacingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ForwardFacingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public abstract ProcessedResponse ToResponse();
    }
}
