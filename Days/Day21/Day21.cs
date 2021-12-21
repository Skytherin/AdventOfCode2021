using System;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using AdventOfCode2021.Utils;
using FluentAssertions;

namespace AdventOfCode2021.Days.Day21
{
    public class Day21: IAdventOfCode
    {
        public void Run()
        {
            //Part1();
            Part2();
        }

        private void Part1()
        {
            var p1score = 0;
            var p2score = 0;
            var p1pos = 6;
            var p2pos = 3;

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

                p1pos = Increment(p1pos, dieTotal, 10);
                p1score += p1pos;

                if (p1score >= 1000) break;

                dieTotal = 0;
                foreach (var _ in Enumerable.Range(0, 3))
                {
                    dieTotal += dieFace;
                    dieFace = Increment(dieFace, 1, 100);
                    dieRolled += 1;
                }

                p2pos = Increment(p2pos, dieTotal, 10);
                p2score += p2pos;

                if (p2score >= 1000) break;
            }

            // Part 1: 752745
        }

        private void Part2()
        {
            var temp = FindCombinations(6, 3, 21, 21);
            temp.Item1.Should().Be(309196008717909);
        }

        // returns number of turns p1 wins, number of turns p2 wins
        private (long, long) FindCombinations(int start1, int start2, int remainingScore1, int remainingScore2)
        {
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