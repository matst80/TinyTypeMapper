using System;
using System.Runtime.Serialization;

namespace TinyMapper
{
    [Serializable]
    public class MapperAlreadyDefinedException : Exception
    {
        public MapperAlreadyDefinedException()
        {
        }

        public MapperAlreadyDefinedException(string message) : base(message)
        {
        }

        public MapperAlreadyDefinedException(Type fromType, Type toType) : base($"Mapper between {fromType.FullName} to {toType.FullName} already exists")
        {
        }

        public MapperAlreadyDefinedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MapperAlreadyDefinedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}