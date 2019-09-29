using System;
using System.Runtime.Serialization;

namespace TinyMapper
{
    [Serializable]
    public class MapperAlreadyDefinedException : Exception
    {

        public MapperAlreadyDefinedException(Type fromType, Type toType) : base($"Mapper between {fromType.FullName} to {toType.FullName} already exists")
        {
        }
    }
}