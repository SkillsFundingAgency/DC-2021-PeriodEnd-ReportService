using System;
using System.Collections.Generic;
using System.Linq;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Extensions
{
    public static class IEnumerableExtensions
    {
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