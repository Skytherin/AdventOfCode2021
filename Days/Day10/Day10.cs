using System.Collections.Generic;
using System.Linq;
using AdventOfCode2021.Utils;
using JetBrains.Annotations;

namespace AdventOfCode2021.Days.Day10
{
    [UsedImplicitly]
    public class Day10 : AdventOfCode<List<string>>
    {
        public override string Example => @"[({(<(())[]>[[{[]{<()<>>
[(()[<>])]({[<{<<[]>>(
{([(<{}[<>[]}>{[]{[(<()>
(((({<>}<{<{<>}{[]{[]{}
[[<[([]))<([[{}[[()]]]
[{[{({}]{}}([{[{{{}}([]
{<[[]]>}<{[{[{[]{()[[[]
[<(<(<(<{}))><([]([]()
<{([([[(<>()){}]>(<<{{
<{([{{}}[<[[[<>{}]]]>[]]";

        public override List<string> Parse(string s) => s.Lines();

        [TestCase(Input.Example, 26397)]
        [TestCase(Input.File, 318081L)]
        public override long Part1(List<string> input)
        {
            return input.Sum(SyntaxCheck);
        }

        [TestCase(Input.Example, 288957)]
        [TestCase(Input.File, 4361305341)]
        public override long Part2(List<string> input)
        {
            var scores = input.Where(line => SyntaxCheck(line) == 0)
                .Select(line => Autocomplete(line))
                .OrderBy(it => it)
                .ToList();

            return scores[scores.Count / 2];
        }

        private long Autocomplete(string line)
        {
            var syntax = new Dictionary<char, char>
            {
                { '(', ')' },
                { '[', ']' },
                { '{', '}' },
                { '<', '>' },
            };
            var openers = syntax.Keys.ToList();
            var scores = new Dictionary<char, int>
            {
                { '(', 1 },
                { '[', 2 },
                { '{', 3 },
                { '<', 4 }
            };

            var stack = new List<char>();
            foreach (var c in line)
            {
                if (openers.Contains(c))
                {
                    stack.Add(c);
                    continue;
                }

                stack.Pop();
            }

            var score = 0L;
            while (stack.Any())
            {
                var top = stack.Pop();
                score = score * 5 + scores[top];
            }

            return score;
        }

        private int SyntaxCheck(string line)
        {
            var syntax = new Dictionary<char, char>
            {
                { '(', ')' },
                { '[', ']' },
                { '{', '}' },
                { '<', '>' },
            };
            var openers = syntax.Keys.ToList();
            var scores = new Dictionary<char, int>
            {
                { ')', 3 },
                { ']', 57 },
                { '}', 1197 },
                { '>', 25137 }
            };

            var stack = new List<char>();
            foreach (var c in line)
            {
                if (openers.Contains(c))
                {
                    stack.Add(c);
                    continue;
                }

                if (!stack.Any()) return scores[c];

                var top = stack.Pop();
                if (c != syntax[top])
                {
                    return scores[c];
                }
            }

            return 0;
        }
    }
}