using System;
using System.Collections.Generic;
using System.Linq;
using AdventOfCode2021.Utils;
using JetBrains.Annotations;

namespace AdventOfCode2021.Days.Day09
{
    [UsedImplicitly]
    public class Day09 : AdventOfCode<int[][]>
    {
        public override int[][] Parse(string s) => s.Lines().Select(line => line.Select(c => Convert.ToInt32($"{c}")).ToArray()).ToArray();

        [TestCase(Input.Example, 15)]
        [TestCase(Input.File, 591)]
        public override long Part1(int[][] input)
        {
            var sum = 0;
            for (var r = 0; r < input.Length; r++)
            {
                for (var c = 0; c < input[0].Length; c++)
                {
                    if (IsLowPoint(input, r, c))
                    {
                        sum += 1 + input[r][c];
                    }
                }
            }

            return sum;
        }

        [TestCase(Input.Example, 1134)]
        [TestCase(Input.File, 1113424)]
        public override long Part2(int[][] input)
        {
            var data = input.Select(row => row.Select(col => new Day9Data(col)).ToArray()).ToArray();

            for (var r = 0; r < input.Length; r++)
            {
                for (var c = 0; c < input[0].Length; c++)
                {
                    ComputeBasin(data, r, c);
                }
            }

            return data.SelectMany(it => it)
                .Where(it => it.BasinNumber is not null)
                .GroupBy(it => it.BasinNumber)
                .Select(it => it.Count())
                .OrderByDescending(it => it)
                .Take(3)
                .Aggregate((accum, current) => accum * current);
        }

        private long? ComputeBasin(Day9Data[][] input, int row, int col)
        {
            var me = input[row][col];
            if (me.Height == 9 || me.BasinNumber != null) return me.BasinNumber;

            var lowerAdjacent = Adjacents(input, row, col)
                .Where(adjacent => adjacent.Value.Height < me.Height)
                .OrderBy(it => it.Value.Height)
                .Take(1)
                .ToList();

            if (lowerAdjacent.Any())
            {
                var temp = lowerAdjacent.Single();
                me.BasinNumber = temp.Value.BasinNumber ?? ComputeBasin(input, temp.Row, temp.Col);
            }
            else
            {
                me.BasinNumber = row * input[0].Length + col;
            }

            return me.BasinNumber;
        }

        private IEnumerable<(int Row, int Col, T Value)> Adjacents<T>(T[][] array, int row, int col)
        {
            var rows = array.Length;
            var columns = array[0].Length;
            if (row > 0) yield return (row - 1, col, array[row - 1][col]);
            if (row < rows - 1) yield return (row + 1, col, array[row + 1][col]);
            if (col > 0) yield return (row, col - 1, array[row][col - 1]);
            if (col < columns - 1) yield return (row, col + 1, array[row][col + 1]);
        }

        private bool IsLowPoint(int[][] input, int row, int col)
        {
            var me = input[row][col];

            return Adjacents(input, row, col).Select(it => it.Value).All(adjacent => me < adjacent);
        }
    }

    public class Day9Data
    {
        public readonly long Height;
        public long? BasinNumber { get; set; }

        public Day9Data(long height)
        {
            Height = height;
        }
    }
}