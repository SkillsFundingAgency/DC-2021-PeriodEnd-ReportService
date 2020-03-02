using System;
using System.Collections.Generic;
using System.Linq;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Extensions
{
    public static class IEnumerableExtensions
    {
        public static int DistinctByCount<TSource>(this IEnumerable<TSource> source, Func<TSource, string> keySelector)
        {
            return source.Select(keySelector).Distinct(StringComparer.OrdinalIgnoreCase).Count();
        }

        public static T MaxOrDefault<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.DefaultIfEmpty().Max();
        }

        public static TResult MaxOrDefault<T, TResult>(this IEnumerable<T> enumerable, Func<T, TResult> func)
        {
            return enumerable.Select(func).DefaultIfEmpty().Max();
        }

        public static TResult MinOrDefault<T, TResult>(this IEnumerable<T> enumerable, Func<T, TResult> func)
        {
            return enumerable.Select(func).DefaultIfEmpty().Min();
        }

        public static T[] ToFixedLengthArray<T>(this IEnumerable<T> enumerable, int size)
        {
            var array = new T[size];

            var collection = enumerable?.ToArray() ?? Array.Empty<T>();

            var iterations = Math.Min(size, collection.Length);

            for (var i = 0; i < iterations; i++)
            {
                array[i] = collection[i];
            }

            return array;
        }
    }
}
