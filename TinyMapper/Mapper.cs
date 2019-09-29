using System;
using System.Threading.Tasks;

namespace TinyMapper
{
    /// <summary>
    /// Represents a mapping between types and the conversion function.
    /// </summary>
    internal sealed class Mapper
    {
        public Type From { get; set; }
        public Type To { get; set; }
        public Func<object, Task<object>> Converter { get; set; }

        /// <summary>
        /// Checks if a mapper is equal to another based on the types,
        /// ignoring the converter
        /// </summary>
        public bool Matches(Type from, Type to) => (from.Equals(From) && to.Equals(To));
    }
}
