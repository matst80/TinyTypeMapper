using System;

namespace TinyMapper
{
    /// <summary>
    /// Attribute used to manually specify what type a value
    /// should convert to.
    /// </summary>
    public class MapToAttribute : Attribute
    {
        public string PropertyName { get; }

        public MapToAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }
}
