using System;
using System.Collections.Generic;
using System.Linq;
using AdventOfCode2021.Utils;
using FluentAssertions;
using FluentAssertions.Common;
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

        //[TestCase(Input.Example, 40)]
        //[TestCase(Input.File, 562)]
        public override long Part1(List<List<int>> input)
        {
            return PathFind(input);
        }

        //[TestCase(Input.Example, 315)]
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

            return PathFind(temp);
        }

        private long PathFind(List<List<int>> grid)
        {
            var rows = grid.Count;
            var columns = grid[0].Count;
            var goal = new Position(rows-1, columns-1);
            var open = new LinkedList<Day15Node>();
            open.AddFirst(new Day15Node(Position.Zero, new List<Position> { Position.Zero }, 0, 0));

            var shortestRoutes = new Dictionary<Position, long>();

            while (open.Any())
            {
                var current = open.Shift();

                if (shortestRoutes.TryGetValue(current.Head, out var shortest))
                {
                    if (shortest <= current.TrueDistance) continue;
                }

                shortestRoutes[current.Head] = current.TrueDistance;

                foreach (var adjacent in Adjacents(current.Head, rows, columns))
                {
                    if (shortestRoutes.TryGetValue(adjacent, out var shortest2))
                    {
                        if (shortest2 <= current.TrueDistance + grid[(int)goal.Y][(int)goal.X]) continue;
                    }

                    shortestRoutes[current.Head] = current.TrueDistance;

                    if (adjacent.Equals(goal)) return current.TrueDistance + grid[(int)goal.Y][(int)goal.X];
                    if (current.Visited.Contains(adjacent)) continue;
                    AddSorted(open, current.AddPoint(adjacent, grid));
                }
            }

            throw new ApplicationException();
        }

        private void AddSorted(LinkedList<Day15Node> open, Day15Node newItem)
        {
            var sortValue = newItem.TrueDistance + newItem.EstimatedDistance;
            for (var current = open.First; current != null; current = current.Next)
            {
                if ((current.Value.EstimatedDistance + current.Value.TrueDistance) <= sortValue) continue;
                open.AddBefore(current, newItem);
                return;
            }

            open.AddLast(newItem);
        }

        private IEnumerable<Position> Adjacents(Position start, int rows, int columns)
        {
            return start.Orthogonals().Where(adjacent => adjacent.X >= 0 && adjacent.Y >= 0 && adjacent.X < columns && adjacent.Y < rows);
        }
    }

    public record Day15Node(Position Head, List<Position> Visited, long TrueDistance, long EstimatedDistance);

    public static class Day15Extensions
    {
        public static Day15Node AddPoint(this Day15Node self, Position p, List<List<int>> grid)
        {
            return new Day15Node(p, self.Visited.Append(p).ToList(), self.TrueDistance + grid[(int)p.Y][(int)p.X],
                EstimateDistance(p, grid));
        }

        private static long EstimateDistance(Position p, List<List<int>> grid)
        {
            var rows = grid.Count;
            var columns = grid[0].Count;
            var goal = new Position(rows - 1, columns - 1);
            return p.ManhattanDistance(goal);
        }
    }
}