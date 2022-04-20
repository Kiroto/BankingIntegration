using BankingIntegration.HTTP;
using BankingIntegration.HTTP.Exceptions;
using System;
using System.Runtime.Serialization;

namespace BankingIntegration
{
    [Serializable]
    internal class TransactionQueuedException : ForwardFacingException
    {
        public TransactionQueuedException()
        {
        }

        public TransactionQueuedException(string message) : base(message)
        {
        }

        public TransactionQueuedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TransactionQueuedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override ProcessedResponse ToResponse()
        {
            throw new NotImplementedException();
        }
    }
}