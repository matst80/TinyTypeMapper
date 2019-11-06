using System;
using System.Threading.Tasks;

namespace TinyMapper.Handler
{
    public static partial class MappingHandler
    {
        /// <summary>
        /// Converts a source value from one type to another.
        ///
        /// Requires that a mapping between the types has been registered.
        /// Since mappings can perform asynchronous work, the conversion is also
        /// asynchronous.
        /// </summary>
        ///
        /// <example>
        /// -- Register a mapping
        /// AddMapping<string, int>(source => source.Length);
        ///
        /// -- Perform conversion
        /// ConvertAsync<string>(1337) // Returns 4
        ///
        /// </example>
        /// <param name="data">Value to convert</param>
        /// <param name="afterMapping">Optional function to run after the mapping to perform additional transformations.</param>
        /// <typeparam name="TTo">The type to convert the value to</typeparam>
        /// <returns>A converted value</returns>
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
        
        public static TTo Convert<TTo>(object data, Func<TTo, Task<TTo>> afterMapping = null) => ConvertAsync(data, afterMapping).GetAwaiter().GetResult();


    }
}