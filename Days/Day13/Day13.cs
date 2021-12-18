using System;
using System.Collections.Generic;
using System.Linq;
using AdventOfCode2021.Utils;
using JetBrains.Annotations;

namespace AdventOfCode2021.Days.Day13
{
    [UsedImplicitly]
    public class Day13 : AdventOfCode<Day13Data>
    {
        public override string Example => @"6,10
0,14
9,10
0,3
10,4
4,11
6,0
6,12
4,1
0,13
10,12
3,4
3,0
8,4
1,10
2,14
8,10
9,0

fold along y=7
fold along x=5";

        public override Day13Data Parse(string input)
        {
            var p = input.Paragraphs().ToList();

            var points = p[0].Select(line => new Position(Convert.ToInt32(line.Split(",")[1]), Convert.ToInt32(line.Split(",")[0])));

            var folds = p[1].Select(StructuredRx.Parse<FoldData>);

            return new Day13Data(points.ToHashSet(), folds.ToList());
        }

        [TestCase(Input.Example, 17)]
        [TestCase(Input.File, 842)]
        public override long Part1(Day13Data input)
        {
            return Fold(input.Points, input.Folds.First()).Count;
        }

        [TestCase(Input.Example, 0)]
        [TestCase(Input.File, 0)]
        public override long Part2(Day13Data input)
        {
            var result = input.Folds.Aggregate(input.Points, Fold);

            var minY = result.Select(it => it.Y).Min();
            var maxY = result.Select(it => it.Y).Max();
            var minX = result.Select(it => it.X).Min();
            var maxX = result.Select(it => it.X).Max();

            Console.WriteLine();
            for (var y = minY; y <= maxY; y++)
            {
                Console.WriteLine();
                for (var x = minX; x <= maxX; x++)
                {
                    if (result.Contains(new Position(y, x))) Console.Write("#");
                    else Console.Write(" ");
                }
            }

            return 0;
        }

        private IReadOnlySet<Position> Fold(IReadOnlySet<Position> points, FoldData foldData)
        {
            return foldData.Axis switch
            {
                FoldAxis.Y => points.Select(p => p.Y > foldData.Value ? new Position(2 * foldData.Value - p.Y, p.X) : p)
                    .ToHashSet(),
                _ => points.Select(p => p.X > foldData.Value ? new Position(p.Y, 2 * foldData.Value - p.X) : p)
                    .ToHashSet()
            };
        }
    }

    public record Day13Data(IReadOnlySet<Position> Points, IReadOnlyList<FoldData> Folds);

    public class FoldData
    {
        [RxFormat(Before = "fold along", After = "=")]
        public FoldAxis Axis { get; set; }
        public int Value { get; set; }
    }

    public enum FoldAxis
    {
        X,
        Y
    }
}