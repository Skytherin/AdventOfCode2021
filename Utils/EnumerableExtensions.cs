using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode2021.Utils
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<(T first, T second)> Pairs<T>(this IEnumerable<T> self)
        {
            var l = self.ToList();
            for (var first = 0; first < l.Count - 1; first++)
            {
                for (var second = first + 1; second < l.Count; second++)
                {
                    yield return (l[first], l[second]);
                }
            }
        }

        public static List<T> ListFromItem<T>(T item)
        {
            return new List<T> { item };
        }

        public static Dictionary<TKey, List<T>> GroupToDictionary<T, TKey>(this IEnumerable<T> input, Func<T, TKey> keyFunc)
        {
            return input.GroupBy(keyFunc).ToDictionary(it => it.Key, it => it.ToList());
        }

        public static T Mode<T>(this IEnumerable<T> self)
            => self.GroupBy(value => value).MaxBy(it => it.Count()).Key;

        // Flips rows and columns, eg:
        // [ [ 1, alpha, foo], [2, beta, bar] ] => [ [1, 2], [alpha, beta], [foo, bar] ]
        public static IEnumerable<List<T>> ZipMany<T>(this IEnumerable<IEnumerable<T>> self)
        {
            return self.Aggregate(new List<List<T>>(), (accum, current) =>
            {
                if (!accum.Any())
                {
                    return current.Select(it => new List<T> { it }).ToList();
                }

                foreach (var z in accum.Zip(current))
                {
                    z.First.Add(z.Second);
                }

                return accum;
            });
        }

        public static TSource MaxBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector) where TSource : notnull
        {
            var comparer = Comparer<TKey>.Default;
            return source.ArgBy(keySelector, lag => comparer.Compare(lag.Current, lag.Previous) > 0);
        }

        public static TSource MinBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector) where TSource : notnull
        {
            var comparer = Comparer<TKey>.Default;
            return source.ArgBy(keySelector, lag => comparer.Compare(lag.Current, lag.Previous) < 0);
        }

        public static TSource ArgBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<(TKey Current, TKey Previous), bool> predicate) where TSource : notnull
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (!source.Any()) new InvalidOperationException("Sequence contains no elements");

            var value = source.First();
            var key = keySelector(value);

            bool hasValue = false;
            foreach (var other in source)
            {
                var otherKey = keySelector(other);
                if (otherKey == null) continue;

                if (hasValue)
                {
                    if (predicate((otherKey, key)))
                    {
                        value = other;
                        key = otherKey;
                    }
                }
                else
                {
                    value = other;
                    key = otherKey;
                    hasValue = true;
                }
            }
            if (hasValue)
            {
                return value;
            }
            throw new InvalidOperationException("Sequence contains no elements");
        }

        public static string Join<T>(this IEnumerable<T> self, string separator = "") =>
            string.Join(separator, self.Select(it => it?.ToString()));

        public static List<string> Lines(this string input) =>
            input.Split("\n")
                .Select(it => it.Trim()).ToList();

        public static IEnumerable<List<string>> SplitIntoParagraphs(this string input)
        {
            var lines = input.Lines();
            var p = new List<string>();
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    if (p.Any()) yield return p;
                    p = new List<string>();
                }
                else
                {
                    p.Add(line);
                }
            }
            if (p.Any()) yield return p;
        }

        public static IEnumerable<T> Range<T>(T first, int count, Func<T, T> generateNext)
        {
            var current = first;
            while (count-- > 0)
            {
                yield return current;
                if (count > 0)
                {
                    current = generateNext(current);
                }
            }
        }

        public static IEnumerable<(T, int)> WithIndices<T>(this IEnumerable<T> self) =>
            self.Select((it, index) => (it, index));

        public static T Pop<T>(this List<T> self)
        {
            if (self.Any())
            {
                var result = self.Last();
                self.RemoveAt(self.Count - 1);
                return result;
            }
            throw new ApplicationException("Attempt to pop empty list.");
        }

        public static T Shift<T>(this List<T> self)
        {
            if (self.Any())
            {
                var result = self.First();
                self.RemoveAt(0);
                return result;
            }
            throw new ApplicationException("Attempt to shift empty list.");
        }
    }
}