using System;
using System.Collections.Generic;
using System.Linq;
using AdventOfCode2021.Utils;
using JetBrains.Annotations;

namespace AdventOfCode2021.Days.Day06
{
    [UsedImplicitly]
    public class Day06 : AdventOfCode<List<long>>
    {
        public override string Example => @"3,4,3,1,2";

        public override List<long> Parse(string s) => s.Split(",").Select(it => Convert.ToInt64(it)).ToList();

        [TestCase(Input.Example, 5934)]
        [TestCase(Input.File, 379114)]
        public override long Part1(List<long> input)
        {
            return Run(input, 80);
        }

        [TestCase(Input.Example, 26984457539)]
        [TestCase(Input.File, 1702631502303)]
        public override long Part2(List<long> input)
        {
            return Run(input, 256);
        }

        private long Run(List<long> input, long days)
        {
            long delayed0 = 0;
            long delayed1 = 0;

            var map = Enumerable.Repeat(0L, 7).ToArray();

            foreach (var i in input)
            {
                map[i] += 1;
            }

            for (var currentDay = 0; currentDay < days; currentDay++)
            {
                var index = currentDay % 7;
                var newlySpawned = map[index];
                map[index] += delayed0;
                delayed0 = delayed1;
                delayed1 = newlySpawned;
            }

            return map.Sum() + delayed0 + delayed1;
        }
    }
}