using System;
using System.Runtime.Serialization;

namespace TinyMapper
{
    [Serializable]
    internal class MapperNotFoundException : Exception
    {
        public MapperNotFoundException()
        {
        }

        public MapperNotFoundException(string message) : base(message)
        {
        }

        public MapperNotFoundException(Type fromType, Type toType) : base($"Converter between {fromType.FullName} to {toType.FullName} not found")
        {
        }

        public MapperNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MapperNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}