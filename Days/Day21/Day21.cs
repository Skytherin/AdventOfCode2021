using System;
using System.Collections.Generic;
using System.Linq;
using AdventOfCode2021.Utils;
using FluentAssertions;

namespace AdventOfCode2021.Days.Day21
{
    public class Day21: IAdventOfCode
    {
        public void Run()
        {
            Part1(4, 8).Should().Be(739785);
            Part1(6, 3).Should().Be(752745);
            Part2(4, 8).Should().Be(444356092776315);
            Part2(6, 3).Should().Be(309196008717909);
        }

        private long Part1(int p1Pos, int p2Pos)
        {
            var p1Score = 0;
            var p2Score = 0;

            var dieFace = 1;
            var dieRolled = 0;

            while (true)
            {
                var dieTotal = 0;
                foreach (var _ in Enumerable.Range(0, 3))
                {
                    dieTotal += dieFace;
                    dieFace = Increment(dieFace, 1, 100);
                    dieRolled += 1;
                }

                p1Pos = Increment(p1Pos, dieTotal, 10);
                p1Score += p1Pos;

                if (p1Score >= 1000) break;

                dieTotal = 0;
                foreach (var _ in Enumerable.Range(0, 3))
                {
                    dieTotal += dieFace;
                    dieFace = Increment(dieFace, 1, 100);
                    dieRolled += 1;
                }

                p2Pos = Increment(p2Pos, dieTotal, 10);
                p2Score += p2Pos;

                if (p2Score >= 1000) break;
            }

            return dieRolled * Math.Min(p1Score, p2Score);
        }

        private long Part2(int p1Pos, int p2Pos)
        {
            var x = FindCombinations(p1Pos, p2Pos, 21, 21);
            return Math.Max(x.Item1, x.Item2);
        }

        private readonly Dictionary<(int, int, int, int), (long, long)> Cache = new();

        // returns number of turns p1 wins, number of turns p2 wins
        private (long, long) FindCombinations(int start1, int start2, int remainingScore1, int remainingScore2)
        {
            var cacheKey = (start1, start2, remainingScore1, remainingScore2);
            if (Cache.TryGetValue(cacheKey, out var value)) return value;

            var odds = new[] { (3, 1), (4, 3), (5, 6), (6, 7), (7, 6), (8, 3), (9, 1) };

            var wins = new long[]{0, 0};

            foreach (var (dieTotal, count) in odds)
            {
                var pos1 = Increment(start1, dieTotal, 10);

                if (pos1 >= remainingScore1)
                {
                    wins[0] += count;
                    continue;
                }

                foreach (var (dieTotal2, count2) in odds)
                {
                    var pos2 = Increment(start2, dieTotal2, 10);

                    if (pos2 >= remainingScore2)
                    {
                        wins[1] += count * count2;
                        continue;
                    }

                    var temp = FindCombinations(pos1, pos2, remainingScore1 - pos1, remainingScore2 - pos2);
                    wins[0] += temp.Item1 * count * count2;
                    wins[1] += temp.Item2 * count * count2;
                }
            }

            Cache[cacheKey] = (wins[0], wins[1]);
            return (wins[0], wins[1]);
        }


        // maps turns to win, to number of times that turn is reached

        int Increment(int initial, int amount, int max)
        {
            var x = (initial + amount) % max;
            if (x == 0) x = max;
            return x;
        }
    }
}