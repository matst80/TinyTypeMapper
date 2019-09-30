using System;
using System.Runtime.Serialization;

namespace TinyMapper
{
    [Serializable]
    public class MapperNotFoundException : Exception
    {
        public MapperNotFoundException(Type fromType, Type toType) : base($"Converter between {fromType.FullName} to {toType.FullName} not found")
        {
        }
    }
}