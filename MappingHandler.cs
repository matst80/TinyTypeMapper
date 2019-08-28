using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Collections.Concurrent;

namespace TinyMapper
{
    public static class MappingHandler
    {
        private static BlockingCollection<Mapper> converters = new BlockingCollection<Mapper>();

        public static void Reset() => converters = new BlockingCollection<Mapper>();

        public static Mapper FindMapper(Type from, Type to)
            => converters.FirstOrDefault(d => d.Matches(from, to));

        public static bool HasMapper(Type from, Type to) => FindMapper(from, to) != null;

        public static bool HasMapper<TFrom, TTo>() => HasMapper(typeof(TFrom), typeof(TTo));

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
                if (!HasMapper(fromType, toType))
                {
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

                    converters.Add(mapper);

                }
                else
                {
                    throw new MapperAlreadyDefinedException(fromType, toType);
                }
            }
        }

        public static void AddMapping<TFrom, TTo>(Func<TFrom, Task<TTo>> converter)
        {
            if (!HasMapper<TFrom, TTo>())
            {
                var mapper = new Mapper()
                {
                    Converter = new Func<object, Task<object>>(async (indata) => indata is TFrom data
                        ? await converter.Invoke(data)
                        : (object)default(TTo)),
                    From = typeof(TFrom),
                    To = typeof(TTo)
                };
                converters.Add(mapper);
            }
            else
            {
                throw new MapperAlreadyDefinedException(typeof(TFrom), typeof(TTo));
            }
        }

        public static void AddMapping<TFrom, TTo>(Func<TFrom, TTo> converter)
        {
            if (!HasMapper<TFrom, TTo>())
            {
                var mapper = new Mapper()
                {
                    Converter = new Func<object, Task<object>>((indata) => Task.FromResult(indata is TFrom data
                        ? converter.Invoke(data)
                        : (object)default(TTo))),
                    From = typeof(TFrom),
                    To = typeof(TTo)
                };
                converters.Add(mapper);
            }
            else
            {
                throw new MapperAlreadyDefinedException(typeof(TFrom), typeof(TTo));
            }
        }

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


        public static async Task<IEnumerable<TTo>> ConvertListAsync<TTo>(object data)
        {
            if (data is IEnumerable array)
            {
                var tasks = new List<Task<TTo>>();
                foreach (var item in array)
                {
                    tasks.Add(ConvertAsync<TTo>(item));
                }
                return await Task.WhenAll(tasks.ToArray());
            }
            return default;
        }

        public static Func<TFrom, Task<TTo>> AutoConverter<TFrom, TTo>(
            Func<TFrom, TTo, Task<TTo>> manualConversion,
            MappingPropertySource baseMappingOn = MappingPropertySource.Target,
            bool throwIfNotFound = false) => async (source) =>
          {
              if (source == null)
              {
                  return default;
              }

              var to = Activator.CreateInstance<TTo>();
              if (baseMappingOn == MappingPropertySource.Target)
              {
                  await CopyReverse(source, to, throwIfNotFound);
              }
              else
              {
                  await Copy(source, to, throwIfNotFound);
              }
              to = await manualConversion(source, to);
              return to;
          };

        public enum MappingPropertySource
        {
            Source,
            Target
        }

        public static Func<TFrom, Task<TTo>> AutoConverter<TFrom, TTo>(MappingPropertySource baseMappingOn = MappingPropertySource.Target, bool requireAllProperties = true) => async (source) =>
            {
                if (source == null)
                {
                    return default;
                }

                var to = Activator.CreateInstance<TTo>();
                if (baseMappingOn == MappingPropertySource.Target)
                {
                    await CopyReverse(source, to, requireAllProperties);
                }
                else
                {
                    await Copy(source, to, requireAllProperties);
                }
                return to;
            };


        private static async Task CopyReverse(object source, object to, bool throwIfNotFound = true)
        {
            var sourceType = source.GetType();
            var toType = to.GetType();
            var sourceProperties = toType.GetProperties();

            foreach (var sourcePropertyInfo in sourceProperties)
            {
                var toProperty = FindProperty(sourcePropertyInfo, sourceType);
                if (toProperty != null)
                {
                    var value = toProperty.GetValue(source);

                    var convertedValue = await ConvertValue(value, sourcePropertyInfo.PropertyType);
                    sourcePropertyInfo.SetValue(to, convertedValue);
                }
                else
                {
                    if (throwIfNotFound)
                    {
                        throw new KeyNotFoundException($"Property mapping for {sourcePropertyInfo.Name} not found");
                    }
                }
            }
        }

        private static async Task Copy(object source, object to, bool throwIfNotFound)
        {
            var sourceType = source.GetType();
            var toType = to.GetType();
            var sourceProperties = sourceType.GetProperties();

            foreach (var sourcePropertyInfo in sourceProperties)
            {
                var toProperty = FindProperty(sourcePropertyInfo, toType);
                if (toProperty != null)
                {
                    var value = sourcePropertyInfo.GetValue(source);

                    toProperty.SetValue(to, await ConvertValue(value, toProperty.PropertyType));
                }
                else
                {
                    if (throwIfNotFound)
                    {
                        throw new KeyNotFoundException($"Property mapping for {sourcePropertyInfo.Name} not found");
                    }
                }
            }
        }

        private static async Task<object> ConvertValue(object value, Type propertyType)
        {
            if (value is null)
            {
                return null;
            }

            var valueType = value.GetType();
            if (valueType.IsAssignableFrom(propertyType))
            {
                return value;
            }
            else
            {
                var mapper = FindMapper(valueType, propertyType);
                if (mapper != null)
                {
                    return await mapper.Converter(value);
                }
            }

            if (value is IDictionary dict)
            {
                var generics = propertyType.GetGenericArguments();
                var destDict = Activator.CreateInstance(propertyType) as IDictionary;
                foreach (var key in dict.Keys)
                {
                    destDict.Add(
                        await ConvertValue(key, generics[0]),
                        await ConvertValue(dict[key], generics[1])
                    );
                }
                return destDict;
            }

            if (value is Enum || propertyType.IsEnum)
            {
                return ConvertEnum(value, propertyType);
            }

            if (value is IEnumerable list)
            {

                return await MakeListOfType(list, propertyType);
            }

            throw new MapperNotFoundException(valueType, propertyType);
        }

        private static object ConvertEnum(object value, Type propertyType)
        {
            return value is string stringValue ? Enum.Parse(propertyType, stringValue, true) : Enum.GetName(value.GetType(), value);
        }

        private static async Task<object> MakeListOfType(IEnumerable listItems, Type listType)
        {
            var itemType = GetListType(listType);
            var list = await GenerateListOfItems(listItems, itemType);

            if (listType.IsArray)
            {
                var returnArray = Array.CreateInstance(itemType, list.Count);
                var i = 0;

                foreach (var item in list)
                {
                    returnArray.SetValue(item, i++);
                }
                return returnArray;
            }
            else if (typeof(IEnumerable).IsAssignableFrom(listType))
            {
                return list;
            }
            throw new ListConverterMissingException(listType);
        }

        private static async Task<IList> GenerateListOfItems(IEnumerable listItems, Type itemType)
        {
            var returnType = typeof(List<>).MakeGenericType(itemType);
            var list = Activator.CreateInstance(returnType) as IList;

            foreach (var item in listItems)
            {
                list.Add(await ConvertValue(item, itemType));
            }

            return list;
        }

        private static Type GetListType(Type propertyType) => propertyType.IsGenericType
                ? propertyType.GetGenericArguments()[0]
                : (propertyType.IsArray
                    ? propertyType.GetElementType()
                    : propertyType);

        private static PropertyInfo FindProperty(PropertyInfo propertyInfo, Type type)
        {
            var propertyName = propertyInfo.Name;
            var customMap = propertyInfo.GetCustomAttribute<MapToAttribute>();
            if (customMap != null)
            {
                propertyName = customMap.PropertyName;
            }
            return type.GetProperty(propertyName);
        }

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
