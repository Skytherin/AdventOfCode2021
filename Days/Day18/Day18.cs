using System;
using System.Collections.Generic;
using System.Linq;
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
            var sfn = S("[1,2]");
            var sfn2 = S("[[3,4],5]");
            (sfn + sfn2).ToString().Should().Be("[[1,2],[[3,4],5]]");

            S("[[[[[9,8],1],2],3],4]").Explode().ToString().Should().Be("[[[[0,9],2],3],4]");
            S("[7,[6,[5,[4,[3,2]]]]]").Explode().ToString().Should().Be("[7,[6,[5,[7,0]]]]");
            S("[[6,[5,[4,[3,2]]]],1]").Explode().ToString().Should().Be("[[6,[5,[7,0]]],3]");
            S("[[3,[2,[1,[7,3]]]],[6,[5,[4,[3,2]]]]]").Explode().ToString().Should().Be("[[3,[2,[8,0]]],[9,[5,[4,[3,2]]]]]");
            S("[[3,[2,[8,0]]],[9,[5,[4,[3,2]]]]]").Explode().ToString().Should().Be("[[3,[2,[8,0]]],[9,[5,[7,0]]]]");

            S("[10,0]").Split().ToString().Should().Be("[[5,5],0]");
            S("[0,[0,11]]").Split().ToString().Should().Be("[0,[0,[5,6]]]");
            S("[0,[12,9]]").Split().ToString().Should().Be("[0,[[6,6],9]]");

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

        private SnailNode S(string s) => SnailNode.FromString(s);
    }

    public class SnailNode
    {
        public readonly IEither<long, (SnailNode Left, SnailNode Right)> Value;
        private string? StringRepCache;
        public SnailNode(long value)
        {
            Value = new FirstEither<long, (SnailNode Left, SnailNode Right)>(value);
        }

        public SnailNode(SnailNode left, SnailNode right)
        {
            Value = new SecondEither<long, (SnailNode Left, SnailNode Right)>((left, right));
        }

        public long? AsLong => Value.Map(it => it, _ => (long?)null);

        public (SnailNode Left, SnailNode Right)? AsSubtree => Value.Map(_ => ((SnailNode Left, SnailNode Right)?)null, it => it);

        public SnailNode Transform(
            Func<SnailNode, bool> match,
            Func<SnailNode, SnailNode> transform)
        {
            if (match(this)) return transform(this);
            if (AsSubtree is { } sfn)
            {
                return new SnailNode(sfn.Left.Transform(match, transform), sfn.Right.Transform(match, transform));
            }
            return this;
        }

        public static SnailNode FromString(string s)
        {
            var original = s;
            s[0].Should().Be('[');
            s.Last().Should().Be(']');
            s = s.Substring(1, s.Length - 2);
            string? remainder2 = null;
            var left = ParseSub(s, out var remainder1) ?? ParseLong(s, out remainder2);
            s = remainder1 ?? remainder2 ?? throw new ApplicationException();
            s[0].Should().Be(',');
            s = s.Substring(1);
            var right = ParseSub(s, out remainder1) ?? ParseLong(s, out remainder2);
            (remainder1 ?? remainder2 ?? throw new ApplicationException()).Length.Should().Be(0);

            return new SnailNode(left, right)
            {
                StringRepCache = original
            };
        }

        private SnailNode(long lhs, long rhs)
        {
            Value = new SecondEither<long, (SnailNode Left, SnailNode Right)>((new SnailNode(lhs), new SnailNode(rhs)));
        }

        public static SnailNode operator +(SnailNode lhs, SnailNode rhs)
        {
            return new SnailNode(lhs, rhs);
        }

        public SnailNode Reduce()
        {
            var result = this;
            while (true)
            {
                var tail = result;
                result = result.Explode();
                if (!ReferenceEquals(result, tail)) continue;
                result = result.Split();
                if (ReferenceEquals(result, tail)) break;
            }

            return result;
        }

        private static SnailNode? ParseSub(string s, out string? remainder)
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
            return FromString(substring);
        }

        public long Magnitude()
        {
            return Value.Map(it => it, it => it.Left.Magnitude() * 3 + it.Right.Magnitude() * 2);
        }

        private static SnailNode ParseLong(string s, out string? remainder)
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
            if (StringRepCache is { } s) return s;
            StringRepCache = Value.Map(it => it.ToString(), it => $"[{it.Left},{it.Right}]");
            return StringRepCache;
        }

        public SnailNode Split()
        {
            var leaf = Nodes()
                .FirstOrDefault(node => node.AsLong! >= 10);

            if (leaf == null) return this;

            var newNumber = new SnailNode((long)leaf.AsLong / 2, (long)leaf.AsLong / 2 + ((long)leaf.AsLong! % 2));

            return Transform(
                node => node == leaf,
                _ => newNumber);
        }

        public SnailNode Explode()
        {
            var leaves = LeavesWithDepth()
                .Where(node => node.Item1 > 4)
                .Take(1)
                .ToList();

            if (leaves.Count == 0) return this;

            var leaf = leaves[0].Item2;
            var left = leaf.AsSubtree!.Value.Left;
            var right = leaf.AsSubtree!.Value.Right;

            var nodes = Nodes().ToList();

            var index = nodes.IndexOf(left);

            var replacements = new Dictionary<SnailNode, SnailNode>
            {
                { leaf, new SnailNode(0) }
            };

            if (index > 0)
            {
                replacements.Add(nodes[index - 1], new SnailNode((long)(nodes[index - 1].AsLong! + left.AsLong!)));
            }

            if (index + 2 < nodes.Count)
            {
                replacements.Add(nodes[index + 2], new SnailNode((long)(nodes[index + 2].AsLong! + right.AsLong!)));
            }

            return Transform(
                node => replacements.ContainsKey(node),
                node => replacements[node]);
        }

        private IEnumerable<(int, SnailNode)> LeavesWithDepth()
        {
            if (AsSubtree is { } subtree)
            {
                var left = subtree.Left;
                var right = subtree.Right;
                if (left.AsLong is { } && right.AsLong is { }) yield return (1, this);

                foreach (var item in left.LeavesWithDepth()) yield return (item.Item1 + 1, item.Item2);

                foreach (var item in right.LeavesWithDepth()) yield return (item.Item1 + 1, item.Item2);
            }
        }

        private IEnumerable<SnailNode> Nodes()
        {
            if (AsLong is { }) yield return this;
            if (AsSubtree is { } subtree)
            {
                foreach (var item in subtree.Left.Nodes()) yield return item;
                foreach (var item in subtree.Right.Nodes()) yield return item;
            }
        }
    }
}