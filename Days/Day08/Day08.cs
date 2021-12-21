using System;
using System.Collections.Generic;
using System.Linq;
using AdventOfCode2021.Utils;
using JetBrains.Annotations;

namespace AdventOfCode2021.Days.Day08
{
    [UsedImplicitly]
    public class Day08 : AdventOfCode<List<SignalPattern>>
    {
        public override List<SignalPattern> Parse(string s) => StructuredRx.ParseLines<SignalPattern>(s);

        [TestCase(Input.Example, 26)]
        [TestCase(Input.File, 543)]
        public override long Part1(List<SignalPattern> input)
        {
            return input.SelectMany(it => it.Outputs).Count(it => new[] {2,3,4,7 }.Contains(it.Length));
        }

        [TestCase(Input.Example, 61229)]
        [TestCase(Input.File, 994266L)]
        public override long Part2(List<SignalPattern> input)
        {
            return input.Select(Decode).Sum();
        }

        private long Decode(SignalPattern pattern)
        {
            var inputs = pattern.Inputs.Select(it => Sort(it)).ToList();

            var one = inputs.Single(it => it.Length == 2);
            var four = inputs.Single(it => it.Length == 4);
            var seven = inputs.Single(it => it.Length == 3);
            var eight = inputs.Single(it => it.Length == 7);

            var two = inputs.Where(it => it.Length == 5).Single(it => it.Intersect(four).Count() == 2);
            var three = inputs.Where(it => it.Length == 5).Single(it => it.ContainsAll(one));
            var five = inputs.Where(it => it.Length == 5).Single(it => it != three && it != two);
            var nine = inputs.Where(it => it.Length == 6).Single(it => it.ContainsAll(four));
            var six = inputs.Where(it => it.Length == 6).Single(it => it != nine &&  it.ContainsAll(five));
            var zero = inputs.Where(it => it.Length == 6).Single(it => it != six && it != nine);

            var known = new List<string>
            {
                zero,
                one,
                two,
                three,
                four,
                five,
                six,
                seven,
                eight,
                nine
            };

            return DecodeOutputs(pattern.Outputs, known);
        }

        private long DecodeOutputs(List<string> outputs, List<string> known)
        {
            return outputs.Select(it => DecodeOutput(Sort(it), known))
                .Aggregate((accum, value) => accum * 10 + value);
        }

        private long DecodeOutput(string needle, List<string> known)
        {
            return known.WithIndices()
                .Where(haystack => haystack.Value == needle)
                .Select(haystack => haystack.Index)
                .Single();
        }

        private string Sort(string s)
        {
            return s.OrderBy(c => c).Join();
        }
    }

    public class SignalPattern
    {
        [RxFormat(After="|")]
        [RxRepeat(10, 10)]
        public List<string> Inputs { get; set; }
        [RxRepeat(4, 4)]
        public List<string> Outputs { get; set; }
    }
}