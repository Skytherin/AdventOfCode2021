using System;
using System.Collections.Generic;
using System.Linq;
using AdventOfCode2021.Utils;
using JetBrains.Annotations;

namespace AdventOfCode2021.Days.Day11
{
    [UsedImplicitly]
    public class Day11 : AdventOfCode<int[][]>
    {
        public override int[][] Parse(string s) => Convert2d(s.Lines(), col => Convert.ToInt32($"{col}"));

        [TestCase(Input.Example, 1656)]
        [TestCase(Input.File, 1637)]
        public override long Part1(int[][] input)
        {
            var flashCount = 0;
            for (var step = 0; step < 100; step++)
            {
                input = Convert2d(input, col => col + 1);
                var flashes = Convert2d(input, _ => false);
                var anyFlashed = true;
                while (anyFlashed)
                {
                    anyFlashed = false;
                    foreach (var cell in Cells(input).Where(it => it.Value > 9 && !flashes[it.Row][it.Col]))
                    {
                        flashes[cell.Row][cell.Col] = true;
                        foreach (var adjacent in Adjacents(input, cell.Row, cell.Col))
                        {
                            input[adjacent.Row][adjacent.Col] += 1;
                        }

                        anyFlashed = true;
                        flashCount += 1;
                    }
                }

                input = Convert2d(input, col => col > 9 ? 0 : col);
            }

            return flashCount;
        }

        [TestCase(Input.Example, 195)]
        [TestCase(Input.File, 242)]
        public override long Part2(int[][] input)
        {
            for (var step = 0; step < 1000; step++)
            {
                input = Convert2d(input, col => col + 1);
                var flashes = Convert2d(input, _ => false);
                var anyFlashed = true;
                while (anyFlashed)
                {
                    anyFlashed = false;
                    foreach (var cell in Cells(input).Where(it => it.Value > 9 && !flashes[it.Row][it.Col]))
                    {
                        flashes[cell.Row][cell.Col] = true;
                        foreach (var adjacent in Adjacents(input, cell.Row, cell.Col))
                        {
                            input[adjacent.Row][adjacent.Col] += 1;
                        }

                        anyFlashed = true;
                    }
                }

                if (Cells(flashes).All(it => it.Value)) return step+1;

                input = Convert2d(input, col => col > 9 ? 0 : col);
            }

            throw new ApplicationException();
        }

        private IEnumerable<(int Row, int Col, T Value)> Adjacents<T>(T[][] array, int row, int col)
        {
            var rows = array.Length;
            var columns = array[0].Length;

            foreach (var dY in new[] { -1, 0, 1 })
            {
                foreach (var dX in new[] { -1, 0, 1 })
                {
                    if (dY != 0 || dX != 0)
                    {
                        var nY = row + dY;
                        var nX = col + dX;
                        if (nY >= 0 && nX >= 0 && nY < rows && nX < columns) yield return (nY, nX, array[nY][nX]);
                    }
                }
            }
        }

        private IEnumerable<(int Row, int Col, T Value)> Cells<T>(T[][] array)
        {
            return array.WithIndices()
                .SelectMany(row => row.Value.WithIndices()
                    .Select(col => (row.Index, col.Index, col.Value)));
        }

        private T2[][] Convert2d<T, T2>(IEnumerable<IEnumerable<T>> input, Func<T, T2> action)
        {
            return input.Select(row => row.Select(action).ToArray()).ToArray();
        }
    }
}