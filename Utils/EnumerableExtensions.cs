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

        public static string Join<T>(this IEnumerable<T> self, string separator = "") =>
            string.Join(separator, self.Select(it => it?.ToString()));

        public static List<string> SplitIntoLines(this string input) =>
            input.Split("\n")
                .Select(it => it.Trim()).ToList();

        public static IEnumerable<List<T>> Permute<T>(this IEnumerable<T> input)
        {
            var original = input.ToList();
            if (original.Count == 0)
            {
                yield return new List<T>();
                yield break;
            }
            foreach (var index in Enumerable.Range(0, original.Count))
            {
                var modified = original.ToList();
                var first = original[index];
                modified.RemoveAt(index);
                foreach (var permutation in Permute(modified))
                {
                    permutation.Insert(0, first);
                    yield return permutation;
                }
            }
        }

        public static IEnumerable<List<T>> Subsets<T>(this IEnumerable<T> input)
        {
            var original = input.ToList();
            if (original.Count == 0)
            {
                yield return new List<T>();
                yield break;
            }

            var mask = Enumerable.Repeat(0, original.Count);

            foreach (var increment in mask.Increments(0, 1, it => it + 1))
            {
                yield return increment.Zip(original)
                    .Where(it => it.First == 1)
                    .Select(it => it.Second)
                    .ToList();
            }
        }

        public static IEnumerable<List<T>> Runs<T>(this IEnumerable<T> input)
        {
            var original = input.ToList();
            if (original.Count == 0)
            {
                yield return new List<T>();
                yield break;
            }

            var runKey = original[0];
            var count = 1;
            foreach (var current in original.Skip(1))
            {
                if (current.Equals(runKey))
                {
                    count += 1;
                }
                else
                {
                    yield return Enumerable.Repeat(runKey, count).ToList();
                    runKey = current;
                    count = 1;
                }
            }
            yield return Enumerable.Repeat(runKey, count).ToList();
        }

        public static IEnumerable<List<T>> Increments<T>(this IEnumerable<T> input,
            T firstElement, T lastElement, Func<T, T> incrementFunc)
        {
            var original = input.ToList();
            while (true)
            {
                var i = original.Count - 1;
                while (i >= 0)
                {
                    if (original[i].Equals(lastElement))
                    {
                        original[i] = firstElement;
                        i -= 1;
                        if (i < 0) yield break;
                        continue;
                    }

                    original[i] = incrementFunc(original[i]);
                    yield return original.ToList();
                    break;
                }
            }
        }

        public static IEnumerable<List<T>> Increments<T>(this IEnumerable<T> input,
            T firstElement, Func<int, T> lastElement, Func<T, T> incrementFunc)
        {
            var original = input.ToList();
            yield return original;
            while (true)
            {
                var i = original.Count - 1;
                while (i >= 0)
                {
                    if (original[i].Equals(lastElement(i)))
                    {
                        original[i] = firstElement;
                        i -= 1;
                        if (i < 0) yield break;
                        continue;
                    }

                    original[i] = incrementFunc(original[i]);
                    yield return original.ToList();
                    break;
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

        public static IEnumerable<(T Value, int Length)> RunLengthEncode<T>(this IEnumerable<T> self)
        {
            var temp = new List<T>();
            var length = 0;
            foreach (var t in self)
            {
                if (!temp.Any())
                {
                    temp.Add(t);
                    length = 1;
                }
                else if (temp[0]!.Equals(t))
                {
                    length += 1;
                }
                else
                {
                    yield return (temp[0], length);
                    temp.Clear();
                    temp.Add(t);
                    length = 1;
                }
            }
            if (length > 0) yield return (temp[0], length);
        }
    }
}