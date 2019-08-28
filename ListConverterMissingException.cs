using System;
using System.Runtime.Serialization;

namespace TinyMapper
{
    [Serializable]
    internal class ListConverterMissingException : Exception
    {
        public ListConverterMissingException()
        {
        }

        public ListConverterMissingException(Type listType) : base($"Converting list to {listType.FullName} could not be done")
        {
        }

        public ListConverterMissingException(string message) : base(message)
        {
        }

        public ListConverterMissingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ListConverterMissingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}