using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace CodeTrip.Utils.ConfigFileChanger.Extensions
{
    public static class EnumerableExtensions
    {
        

        public static IEnumerable<T> OrIfNoneThen<T>(this IEnumerable<T> enumerable, params T[] ifNone)
        {
            return enumerable.Any() ? enumerable : ifNone;
        }
    }
}
