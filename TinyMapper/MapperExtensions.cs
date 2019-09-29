using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TinyMapper.Handler;

namespace TinyMapper
{
    public static class MapperExtensions
    {
        [Obsolete("You should await the async method instead", false)]
        public static TTo Convert<TTo>(this object instance, Func<TTo, Task<TTo>> afterMapping = null) => MappingHandler.Convert(instance, afterMapping);

        public static Task<TTo> ConvertAsync<TTo>(this object instance, Func<TTo, Task<TTo>> afterMapping = null) => MappingHandler.ConvertAsync(instance, afterMapping);

        public static Task<IEnumerable<TTo>> ConvertListAsync<TTo>(this object instance) => MappingHandler.ConvertListAsync<TTo>(instance);
    }
}
