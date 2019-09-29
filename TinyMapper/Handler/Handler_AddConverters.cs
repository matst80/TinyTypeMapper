using System;
using System.Reflection;
using System.Threading.Tasks;

namespace TinyMapper.Handler
{
    public static partial class MappingHandler
    {
        public static void AddConverters(object instance)
        {
            var type = instance.GetType();
            foreach (var prp in type.GetProperties())
            {
                var converter = prp.GetCustomAttribute<TypeConverterAttribute>();
                if (converter != null && prp.PropertyType.IsGenericType)
                {
                    var fromto = prp.PropertyType.GetGenericArguments();
                    var fromType = fromto[0];
                    var toType = fromto[1];

                    var converterFunc = prp.GetValue(instance, null) as Delegate;
                    AddMapping(converterFunc);

                    //if (!HasMapper(fromType, toType))
                    //{
                    //    var mapper = new Mapper()
                    //    {
                    //        Converter = new Func<object, Task<object>>(async (indata) => await (converterFunc.DynamicInvoke(indata) as Task<object>)),
                    //        From = fromType,
                    //        To = toType
                    //    };
                    //    converters.Add(mapper);
                    //}
                    //else
                    //{
                    //    throw new MapperAlreadyDefinedException(fromType, toType);
                    //}
                }
            }
        }
    }
}