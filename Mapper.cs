using System;
using System.Threading.Tasks;

namespace TinyMapper
{
    public class Mapper
    {
        public Type From { get; set; }
        public Type To { get; set; }

        public bool Matches(Type from, Type to) => (from.Equals(From) && to.Equals(To));

        public Func<object, Task<object>> Converter { get; set; }
    }
}
