using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TinyMapper.Handler
{
    public enum MappingPropertySource
    {
        Source,
        Target
    }
    public static partial class MappingHandler
    {
        public delegate bool MappingOverwritten(Type from, Type to);
        public static event MappingOverwritten OnMappingOverwrite;

        private static BlockingCollection<Mapper> converters = new BlockingCollection<Mapper>();
        public static bool HasMapper(Type from, Type to) => FindMapper(from, to) != null;
        public static bool HasMapper<TFrom, TTo>() => HasMapper(typeof(TFrom), typeof(TTo));
        public static Mapper FindMapper(Type from, Type to)
            => converters.FirstOrDefault(d => d.Matches(from, to));

        public static void Reset() => converters = new BlockingCollection<Mapper>();

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
    }
}