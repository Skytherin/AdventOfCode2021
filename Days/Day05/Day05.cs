using System;
using System.Collections.Generic;
using System.Linq;
using AdventOfCode2015.Utils;
using AdventOfCode2021.Utils;
using JetBrains.Annotations;

namespace AdventOfCode2021.Days.Day05
{
    [UsedImplicitly]
    public class Day05 : AdventOfCode<List<Day05Input>>
    {
        public override string Example => @"0,9 -> 5,9
8,0 -> 0,8
9,4 -> 3,4
2,2 -> 2,1
7,0 -> 7,4
6,4 -> 2,0
0,9 -> 2,9
3,4 -> 1,4
0,0 -> 8,8
5,5 -> 8,2";

        public override List<Day05Input> Parse(string s) => StructuredRx.ParseLines<Day05Input>(s);

        [TestCase(Input.Example, 5)]
        [TestCase(Input.File, 6225)]
        public override int Part1(List<Day05Input> input)
        {
            bool IsOrthogonal(Day05Input it) => it.X1 == it.X2 || it.Y1 == it.Y2;
            return Part2(input.Where(IsOrthogonal).ToList());
        }

        [TestCase(Input.Example, 12)]
        [TestCase(Input.File, 22116)]
        public override int Part2(List<Day05Input> input)
        {
            var grid = new Dictionary<Position, int>();
            foreach (var line in input)
            {
                var dX = MoreMath.Sign(line.X2 - line.X1);
                var dY = MoreMath.Sign(line.Y2 - line.Y1);
                var stepsX = MoreMath.Abs(line.X2 - line.X1) + 1;
                var stepsY = MoreMath.Abs(line.Y2 - line.Y1) + 1;
                var steps = Math.Max(stepsX, stepsY);
                for (var step = 0; step < steps; step++)
                {
                    var point = new Position(line.X1 + step * dX, line.Y1 + step * dY);
                    grid[point] = grid.GetValueOrDefault(point) + 1;
                }
            }

            return grid.Values.Count(it => it > 1);
        }
    }

    public class Day05Input
    {
        [RxFormat(After = ",")]
        public int X1 { get; set; }
        [RxFormat(After = "->")]
        public int Y1 { get; set; }

        [RxFormat(After = ",")]
        public int X2 { get; set; }
        public int Y2 { get; set; }
    }
}