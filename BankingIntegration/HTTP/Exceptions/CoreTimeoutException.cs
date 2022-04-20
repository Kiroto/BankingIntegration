using System;
using System.Runtime.Serialization;

namespace BankingIntegration
{
    [Serializable]
    internal class CoreTimeoutException : Exception
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
    }
}