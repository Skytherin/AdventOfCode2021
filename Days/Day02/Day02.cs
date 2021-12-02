using System.Collections.Generic;
using System.IO;
using System.Linq;
using AdventOfCode2021.Utils;
using FluentAssertions;
using JetBrains.Annotations;

namespace AdventOfCode2021.Days.Day02
{
    [UsedImplicitly]
    public class Day02 : IAdventOfCode
    {
        private string Input => File.ReadAllText("Days/Day02/Day02Input.txt");

        const string Example = "forward 5\ndown 5\nforward 8\nup 3\ndown 8\nforward 2";

        private List<Day02Data> Parse(string s) => StructuredRx.ParseLines<Day02Data>(s);

        public void Run()
        {
            Part1();
            Part2();
        }

        private void Part1()
        {
            Pilot(Parse(Example))
                .Should().Be(150);

            Pilot(Parse(Input)).Should().Be(1383564);
        }

        private void Part2()
        {
            Aim(Parse(Example))
                .Should().Be(900);

            Aim(Parse(Input)).Should().Be(1488311643);
        }

        private int Pilot(IReadOnlyCollection<Day02Data> directions)
        {
            var depth = directions.Where(it => it.Instruction == Day02Enum.Down).Sum(it => it.Magnitude) -
                        directions.Where(it => it.Instruction == Day02Enum.Up).Sum(it => it.Magnitude);
            var forward = directions.Where(it => it.Instruction == Day02Enum.Forward).Sum(it => it.Magnitude);
            return depth * forward;
        }

        private int Aim(IEnumerable<Day02Data> directions)
        {
            var result = directions.Aggregate(new { Depth = 0, Position = 0, Aim = 0 }, (accum, current) =>
                current.Instruction switch
                {
                    Day02Enum.Down => new { accum.Depth, accum.Position, Aim = accum.Aim + current.Magnitude },
                    Day02Enum.Up => new { accum.Depth, accum.Position, Aim = accum.Aim - current.Magnitude },
                    Day02Enum.Forward => new { Depth = accum.Depth + accum.Aim * current.Magnitude, Position = accum.Position + current.Magnitude, accum.Aim },
                }
            );

            return result.Depth * result.Position;
        }
    }

    public class Day02Data
    {
        public Day02Enum Instruction { get; set; }
        public int Magnitude { get; set; }
    }

    public enum Day02Enum
    {
        Forward,
        Down,
        Up
    }
}