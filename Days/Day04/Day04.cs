using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AdventOfCode2021.Utils;
using FluentAssertions;
using JetBrains.Annotations;

namespace AdventOfCode2021.Days.Day04
{
    [UsedImplicitly]
    public class Day04 : IAdventOfCode
    {
        private string Input => File.ReadAllText("Days/Day04/Day04Input.txt");

        const string Example = @"7,4,9,5,11,17,23,2,0,14,21,24,10,16,13,6,15,25,12,22,18,20,8,19,3,26,1

22 13 17 11  0
 8  2 23  4 24
21  9 14 16  7
 6 10  3 18  5
 1 12 20 15 19

 3 15  0  2 22
 9 18 13 17  5
19  8  7 25 23
20 11 10 24  4
14 21 16 12  6

14 21 17 24  4
10 16 15  9 19
18  8 23 26 20
22 11 13  6  5
 2  0 12  3  7";

        private BingoGame Parse(string s) => new(s);

        public void Run()
        {
            Part1();
            Part2();
        }

        private void Part1()
        {
            Do1(Parse(Example)).Should().Be(4512);
            Do1(Parse(Input)).Should().Be(8580);
        }

        private void Part2()
        {
            Do2(Parse(Example)).Should().Be(1924);
            Do2(Parse(Input)).Should().Be(9576);
        }

        private int Do2(BingoGame game)
        {
            foreach (var call in game.Calls)
            {
                if (game.CallMap.TryGetValue(call, out var callmap))
                {
                    foreach (var boardmap in callmap)
                    {
                        var board = boardmap.Board;
                        if (game.Boards.Contains(board) && board.Mark(boardmap.Row, boardmap.Col))
                        {
                            if (game.Boards.Count == 1)
                            {
                                return call * board.Cells.Where(it => !it.Marked).Sum(it => it.Value);
                            }

                            game.Boards.Remove(board);
                        }
                    }
                }
            }

            throw new ApplicationException();
        }


        private int Do1(BingoGame game)
        {
            foreach (var call in game.Calls)
            {
                if (game.CallMap.TryGetValue(call, out var callmap))
                {
                    foreach (var boardmap in callmap)
                    {
                        var board = boardmap.Board;
                        if (board.Mark(boardmap.Row, boardmap.Col))
                        {
                            return call * board.Cells.Where(it => !it.Marked).Sum(it => it.Value);
                        }
                    }
                }
            }

            throw new ApplicationException();
        }
    }

    public class BingoGame
    {
        public readonly List<int> Calls;
        public readonly List<BingoBoard> Boards = new();
        public readonly Dictionary<int, List<(BingoBoard Board, int Row, int Col)>> CallMap = new();

        public BingoGame(string data)
        {
            var paragraphs = data.Paragraphs().ToList();
            Calls = paragraphs[0].Single().Split(",").Select(it => Convert.ToInt32(it)).ToList();
            foreach (var p in paragraphs.Skip(1))
            {
                var rows = p.Select(line =>
                        line.Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(it => Convert.ToInt32(it))
                            .ToList())
                    .ToList();
                var board = new BingoBoard(rows);
                Boards.Add(board);
                for (var row = 0; row < board.Rows.Count; row++)
                {
                    for (var col = 0; col < board.Columns.Count; col++)
                    {
                        var v = board.Rows[row][col].Value;
                        var d = new List<(BingoBoard Board, int Row, int Col)>();
                        if (CallMap.TryGetValue(v, out var d2))
                        {
                            d = d2;
                        }
                        d.Add((board, row, col));
                        CallMap[v] = d;
                    }
                }
            }
        }
    }

    public class BingoCell
    {
        public readonly int Value;
        public bool Marked;

        public BingoCell(int value)
        {
            Value = value;
        }
    }

    public class BingoBoard
    {
        public readonly List<List<BingoCell>> Rows;
        public readonly List<List<BingoCell>> Columns;

        public List<BingoCell> Cells => Rows.SelectMany(it => it).ToList();

        public BingoBoard(List<List<int>> rows)
        {
            Rows = rows.Select(row => row.Select(col => new BingoCell(col)).ToList()).ToList();
            Columns = Rows.ZipMany().ToList();
        }

        public bool Mark(int row, int col)
        {
            Rows[row][col].Marked = true;
            return Rows[row].All(cell => cell.Marked) || Columns[col].All(cell => cell.Marked);
        }
    }
}