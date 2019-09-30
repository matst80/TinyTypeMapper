using System;
using System.Threading.Tasks;

namespace TinyMapper.Handler
{
    public static partial class MappingHandler
    {
        public static void AddMapping<TFrom, TTo>(Func<TFrom, Task<TTo>> converter)
        {
            var mapper = new Mapper()
            {
                Converter = new Func<object, Task<object>>(async (indata) => indata is TFrom data
                    ? await converter.Invoke(data)
                    : (object)default(TTo)),
                From = typeof(TFrom),
                To = typeof(TTo)
            };
            if (!HasMapper<TFrom, TTo>())
            {
                converters.Add(mapper);
            }
            else
            {
                if (OnMappingOverwrite != null)
                {
                    if (!OnMappingOverwrite(typeof(TFrom), typeof(TTo)))
                        throw new MapperAlreadyDefinedException(typeof(TFrom), typeof(TTo));
                }
                ReplaceMapper(typeof(TFrom), typeof(TTo), mapper);
            }
        }

        private static void ReplaceMapper(Type from, Type to, Mapper mapper)
        {
            converters.Remove(FindMapper(from,to));
            converters.Add(mapper);
        }

        public static void AddMapping<TFrom, TTo>(Func<TFrom, TTo> converter)
        {
            var mapper = new Mapper()
            {
                Converter = new Func<object, Task<object>>((indata) => Task.FromResult(indata is TFrom data
                    ? converter.Invoke(data)
                    : (object)default(TTo))),
                From = typeof(TFrom),
                To = typeof(TTo)
            };

            if (!HasMapper<TFrom, TTo>())
            {
                converters.Add(mapper);
            }
            else
            {
                if (OnMappingOverwrite != null)
                {
                    if (!OnMappingOverwrite(typeof(TFrom), typeof(TTo)))
                        throw new MapperAlreadyDefinedException(typeof(TFrom), typeof(TTo));
                }
                ReplaceMapper(typeof(TFrom), typeof(TTo), mapper);
            }
        }

        public static void AddMapping(params Delegate[] converterDelegates)
        {
            foreach (var mapperDelegate in converterDelegates)
            {
                var args = mapperDelegate.GetType().GetGenericArguments();
                var fromType = args[0];
                var toType = args[1];
                var isTask = false;
                if (typeof(Task).IsAssignableFrom(toType))
                {
                    toType = toType.GetGenericArguments()[0];
                    isTask = true;
                }

                var conv = isTask
                        ? new Func<object, Task<object>>(async (indata) => indata != null
                             ? await (dynamic)(mapperDelegate.DynamicInvoke(indata))
                             : null)
                        : new Func<object, Task<object>>((indata) => indata != null
                             ? Task.FromResult(mapperDelegate.DynamicInvoke(indata))
                             : Task.FromResult<object>(null));

                var mapper = new Mapper()
                {
                    Converter = conv,
                    From = fromType,
                    To = toType
                };

                if (!HasMapper(fromType, toType))
                {
                    converters.Add(mapper);
                }
                else
                {
                    if (OnMappingOverwrite != null)
                    {
                        if (!OnMappingOverwrite(fromType, toType))
                            throw new MapperAlreadyDefinedException(fromType, toType);
                    }
                    ReplaceMapper(fromType, toType, mapper);
                }
            }
        }

    }
}