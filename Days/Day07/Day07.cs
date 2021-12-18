using System;
using System.Collections.Generic;
using System.Linq;
using AdventOfCode2021.Utils;
using JetBrains.Annotations;

namespace AdventOfCode2021.Days.Day07
{
    [UsedImplicitly]
    public class Day07 : AdventOfCode<List<long>>
    {
        public override string Example => @"16,1,2,0,4,2,7,1,2,14";

        public override List<long> Parse(string s) => s.Split(",").Select(it => Convert.ToInt64(it)).ToList();

        [TestCase(Input.Example, 37)]
        [TestCase(Input.File, 342534L)]
        public override long Part1(List<long> input)
        {
            var groups = input.GroupBy(it => it).ToDictionary(it => it.Key, it => it.Count());
            return groups
                .Select(proposed => groups.Aggregate(0L, (accum, item) => accum + LMath.Abs(item.Key - proposed.Key) * item.Value))
                .Min();
        }

        [TestCase(Input.Example, 168)]
        [TestCase(Input.File, 94004208L)]
        public override long Part2(List<long> input)
        {
            var groups = input.GroupBy(it => it).ToDictionary(it => it.Key, it => it.Count());
            return Enumerable.Range((int)groups.Keys.Min(), (int)(groups.Keys.Max() - groups.Keys.Min() + 1))
                .Select(proposed => groups.Aggregate(0L, (accum, item) => accum + LMath.Triangle(LMath.Abs(item.Key - proposed)) * item.Value))
                .Min();
        }
    }
}