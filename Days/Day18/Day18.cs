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
            var start = DateTime.Now;

            void Log()
            {
                var now = DateTime.Now;
                Console.WriteLine((now - start).TotalMilliseconds);
                start = now;
            }

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
            new SnailFishNumber("[0,[12,9]]").Split().ToString().Should().Be("[0,[[6,6],9]]");

            (S("[[[[4,3],4],4],[7,[[8,4],9]]]") + S("[1,1]")).Reduce().ToString().Should().Be("[[[[0,7],4],[[7,8],[6,0]]],[8,1]]");

            var input = this.File()
                .Lines()
                .Select(S)
                .ToList();
            Log();
            input.Aggregate((current, next) => (current + next).Reduce())
                .Magnitude()
                .Should().Be(4124);
            Log();
            input.SelectMany(left => input.Select(right => left == right ? 0 : (left + right).Reduce().Magnitude()))
                .Max().Should().Be(4673);
            Log();
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

        public SnailNode Transform(
            Func<SnailFishNumber, bool> sfnMatch,
            Func<SnailFishNumber, SnailNode> sfnTransform,
            Func<SnailNode, bool> nodeMatch,
            Func<SnailNode, SnailNode> nodeTransform)
        {
            if (AsSnailFishNumber is { } sfn)
            {
                if (sfnMatch(sfn)) return sfnTransform(sfn);
                if (nodeMatch(this)) return nodeTransform(this);
                return new SnailNode(sfn.Transform(sfnMatch, sfnTransform, nodeMatch, nodeTransform));
            }
            if (nodeMatch(this)) return nodeTransform(this);
            return this;
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

        private SnailFishNumber(SnailNode lhs, SnailNode rhs)
        {
            StringRep = $"[{lhs},{rhs}]";
            Left = lhs;
            Right = rhs;
        }

        private SnailFishNumber(SnailFishNumber lhs, SnailFishNumber rhs)
        {
            StringRep = $"[{lhs},{rhs}]";
            Left = new SnailNode(lhs);
            Right = new SnailNode(rhs);
        }

        private SnailFishNumber(long lhs, long rhs)
        {
            StringRep = $"[{lhs},{rhs}]";
            Left = new SnailNode(lhs);
            Right = new SnailNode(rhs);
        }

        public static SnailFishNumber operator +(SnailFishNumber lhs, SnailFishNumber rhs)
        {
            return new SnailFishNumber(lhs, rhs);
        }

        public SnailFishNumber Reduce()
        {
            var result = this;
            while (true)
            {
                var tail = result;
                result = result.Explode();
                if (result.StringRep != tail.StringRep) continue;
                result = result.Split();
                if (result.StringRep == tail.StringRep) break;
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
            long X(SnailNode node) => node.Value.Map(it => it, it => it.Magnitude());
            return X(Left) * 3 + X(Right) * 2;
        }

        public SnailNode ParseLong(string s, out string? remainder)
        {
            var n = s.TakeWhile(IsNumber).Join();
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

        public SnailFishNumber Split()
        {
            var leaf = Nodes()
                .FirstOrDefault(node => node.AsLong! >= 10);

            if (leaf == null) return this;

            var newNumber = new SnailFishNumber((long)leaf.AsLong / 2, (long)leaf.AsLong / 2 + ((long)leaf.AsLong! % 2));

            return Transform(
                _ => false,
                _ => throw new ApplicationException(),
                node => node == leaf,
                _ => new SnailNode(newNumber));
        }

        public SnailFishNumber Explode()
        {
            var leaves = LeavesWithDepth()
                .Where(node => node.Item1 > 4)
                .Take(1)
                .ToList();

            if (leaves.Count == 0) return this;

            var leaf = leaves[0].Item2;
            var left = leaf.Left;
            var right = leaf.Right;

            var nodes = Nodes().ToList();

            var index = nodes.IndexOf(left);

            var replacements = new Dictionary<SnailNode, SnailNode>();

            if (index > 0)
            {
                replacements.Add(nodes[index - 1], new SnailNode((long)(nodes[index - 1].AsLong! + left.AsLong!)));
            }

            if (index + 2 < nodes.Count)
            {
                replacements.Add(nodes[index + 2], new SnailNode((long)(nodes[index + 2].AsLong! + right.AsLong!)));
            }

            return Transform(
                sfn => ReferenceEquals(sfn, leaf),
                _ => new SnailNode(0),
                node => replacements.ContainsKey(node),
                node => replacements[node]);
        }

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
            else if (Left.AsSnailFishNumber is { } left)
            {
                foreach (var item in left.Nodes()) yield return item;
            }

            if (Right.AsLong is { }) yield return Right;
            else if (Right.AsSnailFishNumber is { } right)
            {
                foreach (var item in right.Nodes()) yield return item;
            }
        }

        public SnailFishNumber Transform(
            Func<SnailFishNumber, bool> sfnMatch,
            Func<SnailFishNumber, SnailNode> sfnTransform,
            Func<SnailNode, bool> nodeMatch,
            Func<SnailNode, SnailNode> nodeTransform)

        {
            SnailNode X(SnailNode node) => node.Transform(sfnMatch, sfnTransform, nodeMatch, nodeTransform);
            return new SnailFishNumber(X(Left), X(Right));
        }
    }
}