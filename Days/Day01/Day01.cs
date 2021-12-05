﻿using System;
using System.Collections.Generic;
using System.Linq;
using AdventOfCode2021.Utils;
using JetBrains.Annotations;

namespace AdventOfCode2021.Days.Day01
{
    [UsedImplicitly]
    public class Day01 : AdventOfCode<List<int>>
    {
        public override string Example => @"199
200
208
210
200
207
240
269
260
263";

        public override List<int> Parse(string input) => input.Lines().Select(it => Convert.ToInt32(it)).ToList();

        [TestCase(Input.Example, 7)]
        [TestCase(Input.File, 1709)]

        public override int Part1(List<int> input)
        {
            return CountIncreases(input);
        }

        [TestCase(Input.Example, 5)]
        [TestCase(Input.File, 1761)]
        public override int Part2(List<int> input)
        {
            return CountIncreases(Windows(input));
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