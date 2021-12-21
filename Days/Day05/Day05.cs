using System;
using System.Collections.Generic;
using System.Linq;
using AdventOfCode2021.Utils;
using JetBrains.Annotations;

namespace AdventOfCode2021.Days.Day05
{
    [UsedImplicitly]
    public class Day05 : AdventOfCode<List<Day05Input>>
    {
        public override List<Day05Input> Parse(string s) => StructuredRx.ParseLines<Day05Input>(s);

        [TestCase(Input.Example, 5)]
        [TestCase(Input.File, 6225)]
        public override long Part1(List<Day05Input> input)
        {
            bool IsOrthogonal(Day05Input it) => it.X1 == it.X2 || it.Y1 == it.Y2;
            return Part2(input.Where(IsOrthogonal).ToList());
        }

        [TestCase(Input.Example, 12)]
        [TestCase(Input.File, 22116)]
        public override long Part2(List<Day05Input> input)
        {
            var grid = new Dictionary<Position, long>();
            foreach (var line in input)
            {
                var dX = LMath.Sign(line.X2 - line.X1);
                var dY = LMath.Sign(line.Y2 - line.Y1);
                var stepsX = LMath.Abs(line.X2 - line.X1) + 1;
                var stepsY = LMath.Abs(line.Y2 - line.Y1) + 1;
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
        public long X1 { get; set; }
        [RxFormat(After = "->")]
        public long Y1 { get; set; }

        [RxFormat(After = ",")]
        public long X2 { get; set; }
        public long Y2 { get; set; }
    }
}