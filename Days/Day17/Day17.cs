using System;
using System.Collections.Generic;
using System.Linq;
using AdventOfCode2021.Utils;
using FluentAssertions;

namespace AdventOfCode2021.Days.Day17
{
    public class Day17: IAdventOfCode
    {
        public void Run()
        {
            Console.WriteLine();
            Part2(-5, 20, -10, 30).Max().Should().Be(45);
            Part2(-67 , 155, -117, 182).Max().Should().Be(6786);

            Part2(-5, 20, -10, 30).Count().Should().Be(112);
            Part2(-67, 155, -117, 182).Count().Should().Be(2313);
        }

        private IEnumerable<long> Part2(int top, int left, int bottom, int right)
        {
            for (var yvelocity = bottom; yvelocity <= (-bottom)*2; yvelocity++)
            {
                for (var xvelocity = 1; xvelocity <= right; xvelocity++)
                {
                    var y = 0;
                    var currentYVelocity = yvelocity;
                    var x = 0;
                    var currentXVelocity = xvelocity;
                    var maxY = 0;
                    var foundMaxY = (long?)null;
                    for (var turn = 1; turn < 1000; turn++)
                    {
                        y += currentYVelocity--;
                        maxY = Math.Max(maxY, y);
                        x += Math.Max(0, currentXVelocity--);
                        if (y <= top && y >= bottom && x >= left && x <= right)
                        {
                            foundMaxY = maxY;
                        }

                        if (y < bottom || x > right) break;
                    }

                    if (foundMaxY is {} f) yield return f;
                }
            }
        }
    }
}