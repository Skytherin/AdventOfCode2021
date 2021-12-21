using System;
using System.Collections.Generic;
using System.Linq;
using AdventOfCode2021.Utils;

namespace AdventOfCode2021.Days.Day20
{
    public class Day20 : AdventOfCode<Day20Input>
    {
        private const char Light = '#';
        public override Day20Input Parse(string s)
        {
            var p = s.Paragraphs().ToList();

            var codec = p[0].Flatten().ToList();

            var positions = p[1].WithIndices()
                .SelectMany(row => row.Value.WithIndices()
                    .Select(col => (new Position(row.Index, col.Index), col.Value))).ToList();

            return new(codec, positions);
        }

        [TestCase(Input.Example, 35)]
        [TestCase(Input.File, 5395)]
        public override long Part1(Day20Input input)
        {
            return Run(input, 2);
        }

        [TestCase(Input.Example, 3351)]
        [TestCase(Input.File, 17584L)]
        public override long Part2(Day20Input input)
        {
            return Run(input, 50);
        }

        private long Run(Day20Input input, int repeat)
        {
            var enhanced = input.Image;
            foreach (var i in Enumerable.Range(0, repeat))
            {
                enhanced = Enhance(enhanced, input.Codec, i % 2 == 1);
            }

            return enhanced.Count(it => it.Item2 == Light);
        }

        private IReadOnlyList<(Position Position, char Value)> Enhance(IReadOnlyList<(Position Position, char Value)> image, IReadOnlyList<char> codec, bool addBorder)
        {
            var grid = image.Select(it => it.Position).ToList();
            var minX = grid.Min(p => p.X);
            var maxX = grid.Max(p => p.X);
            var minY = grid.Min(p => p.Y);
            var maxY = grid.Max(p => p.Y);
            if (codec[0] == Light && addBorder)
            {
                var border = new HashSet<Position>();
                for (var x = minX - 2; x <= maxX + 2; x++)
                {
                    border.Add(new(minY - 2, x));
                    border.Add(new(minY - 1, x));
                    border.Add(new(maxY + 1, x));
                    border.Add(new(maxY + 2, x));
                }
                for (var y = minY - 2; y <= maxY + 2; y++)
                {
                    border.Add(new(y, minX - 2));
                    border.Add(new(y, minX - 1));
                    border.Add(new(y, maxX + 1));
                    border.Add(new(y, maxX + 2));
                }

                image = image.Union(border.Select(p => (p, Light))).ToList();
            }

            var d = new Dictionary<Position, int>();
            foreach (var item in image)
            {
                FillIn(item, d);
            }

            var result = d.Select(it => (Position: it.Key, codec[it.Value])).ToList();

            if (codec[0] == Light && addBorder)
            {
                // remove values beyond first border layer
                result = result.Where(p => p.Position.X >= minX - 1 && 
                                           p.Position.X <= maxX + 1 && 
                                           p.Position.Y >= minY - 1 && 
                                           p.Position.Y <= maxY + 1).ToList();
            }

            return result;
        }

        private void FillIn((Position Position, char Value) p, Dictionary<Position, int> d)
        {
            var increment = p.Value == Light ? 1 : 0;
            foreach (var dy in new []{ Vector.North, new Vector(0, 0), Vector.South })
            foreach (var dx in new[] { Vector.West, new Vector(0, 0), Vector.East })
            {
                var neighbor = p.Position + dy + dx;
                d[neighbor] = d.GetValueOrDefault(neighbor) + increment;
                increment *= 2;
            }
        }
    }

    public record Day20Input(IReadOnlyList<char> Codec, IReadOnlyList<(Position, char)> Image);
}