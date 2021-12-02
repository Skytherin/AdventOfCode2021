using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AdventOfCode2021.Utils;
using FluentAssertions;
using JetBrains.Annotations;

namespace AdventOfCode2021.Days.Day01
{
    [UsedImplicitly]
    public class Day01 : IAdventOfCode
    {
        private List<int> Input => File.ReadAllLines("Days/Day01/Day01Input.txt").Select(it => Convert.ToInt32(it)).ToList();

        public void Run()
        {
            Part1();
            Part2();
        }

        private void Part1()
        {
            CountIncreases(new[]{199, 200, 208, 210, 200, 207, 240, 269, 260, 263})
                .Should().Be(7);

            CountIncreases(Input)
                .Should().Be(1709);
        }

        private void Part2()
        {
            Windows(new[] { 199, 200, 208, 210, 200, 207, 240, 269, 260, 263 })
                .Should().Equal(607, 618, 618, 617, 647, 716, 769, 792);

            CountIncreases(Windows(new []{199, 200, 208, 210, 200, 207, 240, 269, 260, 263}))
                .Should().Be(5);

            CountIncreases(Windows(Input))
                .Should().Be(1761);
        }

        private int CountIncreases(IEnumerable<int> input)
        {
            return input.FirstAndRest((first, rest) => rest.Aggregate((0, first),
                (accum, current) => (accum.Item1 + (current > accum.Item2 ? 1 : 0), current))
            ).Item1;
        }

        private IEnumerable<int> Windows(IEnumerable<int> inputs)
        {
            var three = new List<int>();
            var sum = 0;
            foreach (var input in inputs)
            {
                sum += input;
                three.Add(input);
                if (three.Count > 3) sum -= three.Shift();
                if (three.Count == 3) yield return sum;
            }
        }
    }
    public static class Whatever
    {
        public static TResult FirstAndRest<T, TResult>(this IEnumerable<T> list, Func<T, IEnumerable<T>, TResult> action)
        {
            using var enumerator = list.GetEnumerator();
            if (enumerator.MoveNext())
            {
                var first = enumerator.Current;
                return action(first, EnumeratorToEnumerable(enumerator));
            }

            throw new ApplicationException();
        }

        public static TResult ManyAndRest<T, TResult>(this IEnumerable<T> list, int take, Func<List<T>, IEnumerable<T>, TResult> action)
        {
            using var enumerator = list.GetEnumerator();
            var first = new List<T>();
            while (first.Count < take && enumerator.MoveNext())
            {
                first.Add(enumerator.Current);
            }

            if (first.Count == take)
            {
                return action(first, EnumeratorToEnumerable(enumerator));
            }

            throw new ApplicationException();
        }

        private static IEnumerable<T> EnumeratorToEnumerable<T>(IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext()) yield return enumerator.Current;
        }
    }
}