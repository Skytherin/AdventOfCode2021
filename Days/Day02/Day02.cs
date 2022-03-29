using System;
using System.Collections.Generic;
using System.Linq;
using AdventOfCode2021.Utils;
using JetBrains.Annotations;

namespace AdventOfCode2021.Days.Day02
{
    [UsedImplicitly]
    public class Day02 : AdventOfCode<List<Day02Data>>
    {
        public override List<Day02Data> Parse(string s) => StructuredRx.ParseLines<Day02Data>(s);

        [TestCase(Input.Example, 150)]
        [TestCase(Input.File, 1383564)]
        public override long Part1(List<Day02Data> input)
        {
            var depth = input.Where(it => it.Instruction == Day02Enum.Down).Sum(it => it.Magnitude) -
                        input.Where(it => it.Instruction == Day02Enum.Up).Sum(it => it.Magnitude);
            var forward = input.Where(it => it.Instruction == Day02Enum.Forward).Sum(it => it.Magnitude);
            return depth * forward;
        }

        [TestCase(Input.Example, 900)]
        [TestCase(Input.File, 1488311643)]
        public override long Part2(List<Day02Data> input)
        {
            var result = input.Aggregate(new { Depth = 0L, Position = 0L, Aim = 0L }, (accum, current) =>
                current.Instruction switch
                {
                    Day02Enum.Down => new { accum.Depth, accum.Position, Aim = accum.Aim + current.Magnitude },
                    Day02Enum.Up => new { accum.Depth, accum.Position, Aim = accum.Aim - current.Magnitude },
                    Day02Enum.Forward => new { Depth = accum.Depth + accum.Aim * current.Magnitude, Position = accum.Position + current.Magnitude, accum.Aim },
                    _ => throw new ApplicationException()
                }
            );

            return result.Depth * result.Position;
        }
    }

    public class Day02Data
    {
        public Day02Enum Instruction { get; set; }
        public long Magnitude { get; set; }
    }

    public enum Day02Enum
    {
        Forward,
        Down,
        Up
    }
}