using System;
using System.Collections.Generic;
using RZ.Foundation;
using static RZ.Foundation.Prelude;

namespace RZ.App.TwitterDownloader.Extensions
{
    public static class CollectionExtension
    {
        public static Option<T> MaxBy<T>(this IEnumerable<T> seq, Func<T,int> selector) {
            var iter = seq.GetEnumerator();
            if (!iter.MoveNext()) return None<T>();

            var result = iter.Current;
            var maxValue = selector(result);

            while (iter.MoveNext()) {
                var v = selector(iter.Current);
                if (v > maxValue)
                    result = iter.Current;
            }
            return result;
        }
    }
}
