using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace TinyMapper.Handler
{
    public static partial class MappingHandler
    {
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
    }
}