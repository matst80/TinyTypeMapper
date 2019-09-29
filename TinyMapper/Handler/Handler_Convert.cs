using System;
using System.Threading.Tasks;

namespace TinyMapper.Handler
{
    public static partial class MappingHandler
    {
        public static async Task<TTo> ConvertAsync<TTo>(object data, Func<TTo, Task<TTo>> afterMapping = null)
        {
            if (data is null)
            {
                return default;
            }

            var fromType = data.GetType();
            var toType = typeof(TTo);
            var converter = FindMapper(fromType, toType);
            if (converter == null)
            {
                throw new MapperNotFoundException(fromType, toType);
            }

            var ret = (TTo)(await converter.Converter(data));
            if (afterMapping != null)
            {
                ret = await afterMapping.Invoke(ret);
            }

            return ret;
        }

        [Obsolete("You should await the async method instead", false)]
        public static TTo Convert<TTo>(object data, Func<TTo, Task<TTo>> afterMapping = null) => ConvertAsync(data, afterMapping).GetAwaiter().GetResult();


    }
}