using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Runtime.InteropServices.ComTypes;
using AdventOfCode2021.Utils;
using FluentAssertions;

namespace AdventOfCode2021.Days.Day18
{
    public class Day18: IAdventOfCode
    {
        public void Run()
        {
            Console.WriteLine();
            var sfn = new SnailFishNumber("[1,2]");
            var sfn2 = new SnailFishNumber("[[3,4],5]");
            (sfn + sfn2).ToString().Should().Be("[[1,2],[[3,4],5]]");

            new SnailFishNumber("[[[[[9,8],1],2],3],4]").Explode().ToString().Should().Be("[[[[0,9],2],3],4]");
            new SnailFishNumber("[7,[6,[5,[4,[3,2]]]]]").Explode().ToString().Should().Be("[7,[6,[5,[7,0]]]]");
            new SnailFishNumber("[[6,[5,[4,[3,2]]]],1]").Explode().ToString().Should().Be("[[6,[5,[7,0]]],3]");
            new SnailFishNumber("[[3,[2,[1,[7,3]]]],[6,[5,[4,[3,2]]]]]").Explode().ToString().Should().Be("[[3,[2,[8,0]]],[9,[5,[4,[3,2]]]]]");
            new SnailFishNumber("[[3,[2,[8,0]]],[9,[5,[4,[3,2]]]]]").Explode().ToString().Should().Be("[[3,[2,[8,0]]],[9,[5,[7,0]]]]");

            new SnailFishNumber("[10,0]").Split().ToString().Should().Be("[[5,5],0]");
            new SnailFishNumber("[0,[0,11]]").Split().ToString().Should().Be("[0,[0,[5,6]]]");

            (S("[[[[4,3],4],4],[7,[[8,4],9]]]") + S("[1,1]")).Reduce().ToString().Should().Be("[[[[0,7],4],[[7,8],[6,0]]],[8,1]]");

            var input = this.File()
                .Lines()
                .Select(S)
                .ToList();
            
            input.Aggregate((current, next) => (current + next).Reduce())
                .Magnitude()
                .Should().Be(4124);

            input.SelectMany(left => input.Select(right => left == right ? 0 : (left + right).Reduce().Magnitude()))
                .Max().Should().Be(4673);
        }

        private SnailFishNumber S(string s) => new(s);
    }

    public class SnailNode
    {
        public readonly IEither<long, SnailFishNumber> Value;
        public SnailNode(long value)
        {
            Value = new FirstEither<long, SnailFishNumber>(value);
        }

        public SnailNode(SnailFishNumber value)
        {
            Value = new SecondEither<long, SnailFishNumber>(value);
        }

        public long? AsLong => Value.Map(it => it, _ => (long?)null);

        public SnailFishNumber? AsSnailFishNumber => Value.Map(_ => (SnailFishNumber?)null, it => it);

        public override string ToString()
        {
            return Value.Map(it => it.ToString(), it => it.ToString());
        }

        public string ToStringSpecial(Func<SnailNode, bool> matcher, Func<SnailNode, string> transform,
            Func<SnailFishNumber, bool> sfnMatch,
            Func<SnailFishNumber, string> sfnTransform)
        {
            if (matcher(this)) return transform(this);
            return Value.Map(it => it.ToString(), it => it.ToStringSpecial(matcher, transform, sfnMatch, sfnTransform));
        }
    }


    public class SnailFishNumber
    {
        public readonly SnailNode Left;
        public readonly SnailNode Right;
        private readonly string StringRep;

        public SnailFishNumber(string s)
        {
            StringRep = s;
            s[0].Should().Be('[');
            s.Last().Should().Be(']');
            s = s.Substring(1, s.Length - 2);
            string? remainder2 = null;
            Left = ParseSub(s, out var remainder1) ?? ParseLong(s, out remainder2);
            s = remainder1 ?? remainder2 ?? throw new ApplicationException();
            s[0].Should().Be(',');
            s = s.Substring(1);
            Right = ParseSub(s, out remainder1) ?? ParseLong(s, out remainder2);
            (remainder1 ?? remainder2 ?? throw new ApplicationException()).Length.Should().Be(0);
        }

        public static bool operator ==(SnailFishNumber lhs, SnailFishNumber rhs)
        {
            return lhs.StringRep == rhs.StringRep;
        }

        public static bool operator !=(SnailFishNumber lhs, SnailFishNumber rhs)
        {
            return !(lhs == rhs);
        }

        public static SnailFishNumber operator +(SnailFishNumber lhs, SnailFishNumber rhs)
        {
            return new SnailFishNumber($"[{lhs},{rhs}]");
        }

        public SnailFishNumber Reduce()
        {
            var result = this;
            while (true)
            {
                var exploded = result.Explode();
                if (exploded != result)
                {
                    result = exploded;
                    continue;
                }
                var split = result.Split();
                if (split != result)
                {
                    result = split;
                    continue;
                }
                break;
            }

            return result;
        }

        public SnailNode? ParseSub(string s, out string? remainder)
        {
            if (s[0] != '[')
            {
                remainder = null;
                return null;
            }
            var n = 1;
            var substring = "[";
            foreach (var c in s.Skip(1))
            {
                if (c == '[') n++;
                else if (c == ']')
                {
                    n -= 1;
                }

                substring += c;
                if (n == 0) break;
            }

            remainder = s.Substring(substring.Length);
            return new SnailNode(new SnailFishNumber(substring));
        }

        public long Magnitude()
        {
            return Left.Value.Map(it => it, it => it.Magnitude()) * 3
                   + Right.Value.Map(it => it, it => it.Magnitude()) * 2;
        }

        public SnailNode ParseLong(string s, out string? remainder)
        {
            var n = s.TakeWhile(c => IsNumber(c)).Join();
            remainder = s.Substring(n.Length);
            return new SnailNode(Convert.ToInt64(n));
        }

        private static bool IsNumber(char c)
        {
            return new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }.Contains(c);
        }

        public override string ToString()
        {
            return StringRep;
        }

        public string ToStringSpecial(Func<SnailNode, bool> matcher, Func<SnailNode, string> transform, 
            Func<SnailFishNumber, bool> sfnMatch,
            Func<SnailFishNumber, string> sfnTransform)
        {
            if (sfnMatch(this)) return sfnTransform(this);
            return $"[{Left.ToStringSpecial(matcher, transform, sfnMatch, sfnTransform)},{Right.ToStringSpecial(matcher, transform, sfnMatch, sfnTransform)}]";
        }

        public SnailFishNumber Split()
        {
            var leaf = Nodes()
                .FirstOrDefault(node => node.AsLong! >= 10);

            if (leaf == null) return this;

            var newNode = $"[{leaf.AsLong / 2},{(leaf.AsLong / 2) + (leaf.AsLong % 2) }]";

            return new SnailFishNumber(ToStringSpecial(node => node == leaf, _ => newNode, _ => false, _ => ""));
        }

        public SnailFishNumber Explode()
        {
            var leaves = LeavesWithDepth()
                .Where(node => node.Item1 > 4)
                .Take(1)
                .ToList();

            if (leaves.Count == 0) return this;

            var left = leaves[0].Item2.Left;
            var right = leaves[0].Item2.Right;

            var nodes = Nodes().ToList();

            var index = nodes.IndexOf(left);

            // sanity check
            index.Should().BeGreaterThan(-1);
            nodes[index + 1].Should().Be(right);

            var replacements = new Dictionary<SnailNode, SnailNode>();

            if (index > 0)
            {
                replacements.Add(nodes[index - 1], new SnailNode((long)(nodes[index - 1].AsLong! + left.AsLong!)));
            }

            if (index + 2 < nodes.Count)
            {
                replacements.Add(nodes[index + 2], new SnailNode((long)(nodes[index + 2].AsLong! + right.AsLong!)));
            }

            return new SnailFishNumber(ToStringSpecial(
                node => replacements.ContainsKey(node),
                node => replacements[node].ToString(),
                sfn => sfn == leaves[0].Item2,
                _ => "0"));
        }

        //public SnailFishNumber Explode()
        //{
        //    var leaves = LeavesWithDepth()
        //        .Where(node => node.Item1 > 4)
        //        .Take(1)
        //        .ToList();

        //    if (leaves.Count == 0) return this;

        //    var leaf = leaves[0];

        //    var s = ToStringSpecial(node => node == leaf.Item2.Left || node == leaf.Item2.Right, _ => "*");

        //    var leftStarIndex = s.IndexOf('*');
        //    var rightStarIndex = s.LastIndexOf('*');
            
        //    var leftNumber = s.Take(leftStarIndex - 1).WithIndices().Reverse()
        //        .SkipWhile(it => !IsNumber(it.Value)).TakeWhile(it => IsNumber(it.Value)).Reverse().ToList();

        //    var rightNumber = s.WithIndices().Skip(rightStarIndex + 1).SkipWhile(it => !IsNumber(it.Value)).TakeWhile(it => IsNumber(it.Value)).ToList();

        //    if (rightNumber.Any())
        //    {
        //        var rn = Convert.ToInt64(rightNumber.Select(it => it.Value).Join());
        //        s = s.Substring(0, rightNumber[0].Index) + $"{rn + leaf.Item2.Right.AsLong!}" + s.Substring(rightNumber[^1].Index + 1);
        //    }
            
        //    s = s.Substring(0, leftStarIndex - 1) + "0" + s.Substring(rightStarIndex + 2);

        //    if (leftNumber.Any())
        //    {
        //        var ln = Convert.ToInt64(leftNumber.Select(it => it.Value).Join());
        //        s = s.Substring(0, leftNumber[0].Index) + $"{ln + leaf.Item2.Left.AsLong!}" + s.Substring(leftNumber[^1].Index + 1);
        //    }

        //    return new SnailFishNumber(s);
        //}

        private IEnumerable<(int, SnailFishNumber)> LeavesWithDepth()
        {
            if (Left.AsLong is { } && Right.AsLong is { }) yield return (1, this);

            if (Left.AsSnailFishNumber is { } sfn)
            {
                foreach (var item in sfn.LeavesWithDepth()) yield return (item.Item1 + 1, item.Item2);
            }

            if (Right.AsSnailFishNumber is { } sfn2)
            {
                foreach (var item in sfn2.LeavesWithDepth()) yield return (item.Item1 + 1, item.Item2);
            }
        }

        private IEnumerable<SnailNode> Nodes()
        {
            if (Left.AsLong is { }) yield return Left;
            else if (Left.AsSnailFishNumber is { } sfn)
            {
                foreach (var item in sfn.Nodes()) yield return item;
            }

            if (Right.AsLong is { }) yield return Right;
            else if (Right.AsSnailFishNumber is { } sfn2)
            {
                foreach (var item in sfn2.Nodes()) yield return item;
            }
        }

    }
}