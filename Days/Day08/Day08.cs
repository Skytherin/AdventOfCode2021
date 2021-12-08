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
        public override string Example => @"be cfbegad cbdgef fgaecd cgeb fdcge agebfd fecdb fabcd edb | fdgacbe cefdb cefbgd gcbe
edbfga begcd cbg gc gcadebf fbgde acbgfd abcde gfcbed gfec | fcgedb cgb dgebacf gc
fgaebd cg bdaec gdafb agbcfd gdcbef bgcad gfac gcb cdgabef | cg cg fdcagb cbg
fbegcd cbd adcefb dageb afcb bc aefdc ecdab fgdeca fcdbega | efabcd cedba gadfec cb
aecbfdg fbg gf bafeg dbefa fcge gcbea fcaegb dgceab fcbdga | gecf egdcabf bgf bfgea
fgeab ca afcebg bdacfeg cfaedg gcfdb baec bfadeg bafgc acf | gebdcfa ecba ca fadegcb
dbcfg fgd bdegcaf fgec aegbdf ecdfab fbedc dacgb gdcebf gf | cefg dcbef fcge gbcadfe
bdfegc cbegaf gecbf dfcage bdacg ed bedf ced adcbefg gebcd | ed bcgafe cdgba cbgef
egadfb cdbfeg cegd fecab cgb gbdefca cg fgcdab egfdb bfceg | gbdfcae bgc cg cgb
gcafb gcf dcaebfg ecagb gf abcdeg gaef cafbge fdbac fegbdc | fgae cfgab fg bagce";

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

            var nine = inputs.Where(it => it.Length == 6).Where(it => it.ContainsAll(four))

            var top = seven.Except(one).Join();

            var topLeftAndMiddle = four.Except(one).Join();
            var bottomAndBottomLeft = eight.Except(seven).Except(four).ToList();

            var nine = inputs.Where(it => it.Length == 6).Single(it => bottomAndBottomLeft.Contains(eight.Except(it).Single()));
            var bottomLeft = eight.Except(nine).Join();
            var bottom = bottomAndBottomLeft.Except(bottomLeft).Join();

            var zero = inputs.Where(it => it.Length == 6).Single(it => topLeftAndMiddle.Contains(eight.Except(it).Single()));
            var middle = eight.Except(zero).Join();
            var topLeft = topLeftAndMiddle.Except(middle).Join();

            var twoThreeFive = inputs.Where(it => it.Length == 5).ToList();

            var threeWires = seven.Union(middle).Union(bottom).Join();
            var three = twoThreeFive.Single(it => it.All(c => threeWires.Contains(c)));
            var twoFive = twoThreeFive.Where(it => it != three).ToList();

            var two = twoFive.Single(it => it.Contains(bottomLeft.Single()));

            var bottomRight = one.Except(two).Join();
            var topRight = one.Except(bottomRight).Join();

            var five = top.Union(bottom).Union(middle).Union(topLeft).Union(bottomRight).Join();
            var six = five.Union(bottomLeft).Join();

            var known = new List<string>
            {
                Sort(zero),
                Sort(one),
                Sort(two),
                Sort(three),
                Sort(four),
                Sort(five),
                Sort(six),
                Sort(seven),
                Sort(eight),
                Sort(nine)
            };

            return DecodeOutputs(pattern.Outputs, known);
        }

        private long DecodeOutputs(List<string> outputs, List<string> known)
        {
            var temp = outputs.Select(it => DecodeOutput(Sort(it), known)).ToList();
            return temp.Aggregate((accum, value) => accum * 10 + value);
        }

        private long DecodeOutput(string output, List<string> known)
        {
            var i = known.IndexOf(Sort(output));
            if (i < 0) throw new ApplicationException();
            return i;
        }

        private string Sort(string s)
        {
            return s.OrderBy(c => c).Join();
        }
    }

    public enum Segment
    {
        Top,
        Middle,
        Bottom,
        UpperLeft,
        UpperRight,
        LowerLeft,
        LowerRight
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