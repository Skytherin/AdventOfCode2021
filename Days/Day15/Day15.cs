using System;
using System.Collections.Generic;
using System.Linq;
using AdventOfCode2021.Utils;
using JetBrains.Annotations;

namespace AdventOfCode2021.Days.Day15
{
    [UsedImplicitly]
    public class Day15 : AdventOfCode<List<List<int>>>
    {
        public override string Example => @"1163751742
1381373672
2136511328
3694931569
7463417111
1319128137
1359912421
3125421639
1293138521
2311944581";

        public override List<List<int>> Parse(string input) => input.Lines().Select(it => it.Select(c => Convert.ToInt32($"{c}")).ToList()).ToList();

        [TestCase(Input.Example, 40)]
        [TestCase(Input.File, 562)]
        public override long Part1(List<List<int>> input)
        {
            return PathFind2(input);
        }

        [TestCase(Input.Example, 315)]
        [TestCase(Input.File, 2874)]
        public override long Part2(List<List<int>> input)
        {
            var width = input[0].Count;
            var height = input.Count;
            var grid = input.WithIndices().SelectMany(row =>
                row.Value.WithIndices().Select(col => (new Position(row.Index, col.Index), col.Value))).ToList();

            int Func(int i, int incr) => (i + incr) >= 10  ? (i + incr) - 9 : i + incr;

            grid = grid.SelectMany(p => Enumerable.Range(0, 5).Select(offset => (new Position(p.Item1.Y, p.Item1.X + width * offset), Func(p.Item2, offset)))).ToList();
            grid = grid.SelectMany(p => Enumerable.Range(0, 5).Select(offset => (new Position(p.Item1.Y + height * offset, p.Item1.X), Func(p.Item2, offset)))).ToList();

            var temp = grid.GroupBy(it => it.Item1.Y)
                .OrderBy(row => row.Key)
                .Select(row => row.GroupBy(it => it.Item1.X).OrderBy(col => col.Key).SelectMany(col => col.Select(it => it.Value)).ToList())
                .ToList();

            return PathFind2(temp);
        }

        private long PathFind2(List<List<int>> input)
        {
            var rows = input.Count;
            var columns = input[0].Count;
            var goal = new Position(rows - 1, columns - 1);
            var open = new PriorityQueue<(Position Position, long TrueDistance, long EstimatedDistance)>(it => it.EstimatedDistance);
            open.Enqueue((Position.Zero, 0, goal.ManhattanDistance()));
            var closed = new HashSet<Position> { Position.Zero };
            while (open.Count > 0)
            {
                var current = open.Dequeue();
                foreach (var adjacent in Adjacents(current.Position, rows, columns))
                {
                    var adjacentTrueDistance = current.TrueDistance + input[(int)adjacent.Y][(int)adjacent.X];
                    if (adjacent == goal) return adjacentTrueDistance;
                    if (closed.Contains(adjacent)) continue;
                    closed.Add(adjacent);
                    open.Enqueue((adjacent, adjacentTrueDistance, adjacentTrueDistance + goal.ManhattanDistance(adjacent)));
                }
            }

            throw new ApplicationException();
        }


        private IEnumerable<Position> Adjacents(Position start, int rows, int columns)
        {
            return start.Orthogonals().Where(adjacent => adjacent.X >= 0 && adjacent.Y >= 0 && adjacent.X < columns && adjacent.Y < rows);
        }
    }
}