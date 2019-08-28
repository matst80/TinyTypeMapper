using System;

namespace TinyMapper
{
    public class TypeConverterAttribute : Attribute
    {

    }

    public class MapToAttribute : Attribute
    {
        public string PropertyName { get; }

        public MapToAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }
}
