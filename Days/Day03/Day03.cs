using System.Collections.Generic;
using System.IO;
using System.Linq;
using AdventOfCode2021.Utils;
using FluentAssertions;
using JetBrains.Annotations;

namespace AdventOfCode2021.Days.Day03
{
    [UsedImplicitly]
    public class Day03 : IAdventOfCode
    {
        private string Input => File.ReadAllText("Days/Day03/Day03Input.txt");

        const string Example = @"00100
11110
10110
10111
10101
01111
00111
11100
10000
11001
00010
01010";

        private List<string> Parse(string s) => s.SplitIntoLines();

        public void Run()
        {
            Part1();
            Part2();
        }

        private void Part1()
        {
            Do1(Parse(Example)).Should().Be(198);
            Do1(Parse(Input)).Should().Be(3959450);
        }

        private void Part2()
        {
            Do2(Parse(Example)).Should().Be(230);
            Do2(Parse(Input)).Should().Be(7440311);
        }

        private int Do1(IEnumerable<string> inputs)
        {
            var gammaValue = inputs.ZipMany().Select(it => it.Mode()).Join();

            var epsilonValue = gammaValue.Select(it => it == '1' ? '0' : '1').Join();
            var gv = BinaryStringToInt(gammaValue);
            var ev = BinaryStringToInt(epsilonValue);
            return gv * ev;
        }

        private int Do2(List<string> inputs)
        {
            return Reduce(inputs, Day03Enum.Most) * Reduce(inputs, Day03Enum.Least);
        }

        private int Reduce(List<string> inputs, Day03Enum selector)
        {
            for(var position = 0; inputs.Count > 1 && position < inputs[0].Length; position++)
            {
                var d = inputs.GroupToDictionary(it => it[position]);
                var ones = d.GetValueOrDefault('1')?.Count ?? 0;
                var zeroes = d.GetValueOrDefault('0')?.Count ?? 0;
                var w = selector switch
                {
                    Day03Enum.Most => ones >= zeroes ? '1' : '0',
                    _ => zeroes <= ones ? '0' : '1'
                };

                inputs = d[w];
            }

            return BinaryStringToInt(inputs.First());
        }

        private int BinaryStringToInt(IEnumerable<char> input) =>
            input.Aggregate(0, (accum, v) => accum * 2 + (v == '1' ? 1 : 0));
    }


    public enum Day03Enum
    {
        Least,
        Most
    }
}