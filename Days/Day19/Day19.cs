using System;
using System.Collections.Generic;
using System.Linq;
using AdventOfCode2021.Utils;
using FluentAssertions;
using JetBrains.Annotations;

namespace AdventOfCode2021.Days.Day19
{
    [UsedImplicitly]
    public class Day19: AdventOfCode<List<List<Position3d>>>
    {
        private readonly List<Func<Position3d, Position3d>> Orientations = new();

        public Day19()
        {
            foreach(var facingUp in new List<Func<Position3d, Position3d>>()
            {
                p => p, // A
                p => p.RollUp(), // B
                p => p.RollUp().RollUp(), // E
                p => p.RollUp().RollUp().RollUp(), // F
                p => p.RollLeft(), // C
                p => p.RollLeft().RollLeft().RollLeft() // D
            })
            {
                Orientations.Add(p => facingUp(p));
                Orientations.Add(p => facingUp(p).Rotate());
                Orientations.Add(p => facingUp(p).Rotate().Rotate());
                Orientations.Add(p => facingUp(p).Rotate().Rotate().Rotate());
            }
        }

        public override List<List<Position3d>> Parse(string input)
        {
            return input.Paragraphs()
                .Select(p => p.Skip(1).Select(it =>
                {
                    var csv = it.Split(",").Select(n => Convert.ToInt64(n));
                    return new Position3d(csv);
                }).ToList())
                .ToList();
        }

        [TestCase(Input.Example, 79)]
        [TestCase(Input.File, 332)]
        public override long Part1(List<List<Position3d>> scanners)
        {
            var result = Run(scanners, 12);

            return result;
        }

        private long Run(List<List<Position3d>> scanners, int minOverlaps)
        {
            var orients = new Dictionary<(int, int), (Func<Position3d, Position3d> orientation, Position3d offset)>();

            foreach (var choice in scanners.WithIndices().Choose(2))
            {
                Console.WriteLine(choice.Select(it => it.Index).Join(","));
                var overlaps = Overlap(choice[0].Value, choice[1].Value, minOverlaps);
                if (overlaps is not {} overlap) continue;
                orients[(choice[0].Index, choice[1].Index)] = overlap;
                overlaps = Overlap(choice[1].Value, choice[0].Value, minOverlaps);
                orients[(choice[1].Index, choice[0].Index)] = overlaps!.Value;
            }

            var points = scanners[0].Concat(
                    scanners.WithIndices().Skip(1).SelectMany(it => RotateIntoPosition(it.Value, it.Index, orients)))
                .ToHashSet();

            var relativePositions = scanners.WithIndices().Skip(1).Select(it => GetRelativePosition(Position3d.Zero, it.Index, orients));

            var x = new List<Position3d> { Position3d.Zero }.Concat(relativePositions)
                .Choose(2)
                .Select(it => it[0].ManhattanDistance(it[1]))
                .Max();

            return points.Count;
        }

        private Position3d? GetRelativePosition(Position3d startPosition,
            int index,
            Dictionary<(int, int), (Func<Position3d, Position3d> orientation, Position3d offset)> orients,
            HashSet<int>? closed = null)
        {
            var top = closed == null;
            closed ??= new HashSet<int>();
            if (closed.Contains(index)) return null;
            closed.Add(index);
            {
                if (orients.TryGetValue((0, index), out var o))
                {
                    return o.orientation(startPosition) - o.offset;
                }
            }

            foreach (var (key, value) in orients.Where(it => it.Key.Item2 == index))
            {
                var position = GetRelativePosition(value.orientation(startPosition) - value.offset, key.Item1, orients, closed.ToHashSet());
                if (position is {} p)
                {
                    return p;
                }
            }

            if (top) throw new ApplicationException();
            return null;
        }

        private List<Position3d>? RotateIntoPosition(List<Position3d> l,
            int index,
            Dictionary<(int, int), (Func<Position3d, Position3d> orientation, Position3d offset)> orients,
            HashSet<int>? closed = null)
        {
            var top = closed == null;
            closed ??= new HashSet<int>();
            if (closed.Contains(index)) return null;
            closed.Add(index);
            if (orients.TryGetValue((0, index), out var o))
            {
                return Orient(l, o.orientation).Select(it => it - o.offset).ToList();
            }

            foreach (var (key, value) in orients.Where(it => it.Key.Item2 == index))
            {
                var temp = Orient(l, value.orientation).Select(it => it - value.offset).ToList();
                var result = RotateIntoPosition(temp, key.Item1, orients, closed.ToHashSet());
                if (result != null) return result;
            }

            if (top) throw new ApplicationException();
            return null;
        }

        // maps point in list1 to points in list2
        private (Func<Position3d, Position3d> orientation, Position3d offsetBeforeRotation)? Overlap(List<Position3d> list1, List<Position3d> list2, int minOverlaps)
        {
            var relative1 = RelativePositions(list1);

            foreach (var orientation in Orientations)
            {
                var orientated = Orient(list2, orientation);
                var relative2 = RelativePositions(orientated);
            
                var result = new List<(Position3d, Position3d)>();

                var closed = new HashSet<int>();

                for (var p1Index = 0; p1Index < relative1.Count; ++p1Index)
                {
                    for (var p2Index = 0; p2Index < relative2.Count; p2Index++)
                    {
                        if (closed.Contains(p2Index)) continue;
                        if (Commonality(relative1[p1Index], relative2[p2Index]) >= minOverlaps - 1)
                        {
                            result.Add((list1[p1Index], orientated[p2Index]));
                            closed.Add(p2Index);
                            break;
                        }
                    }
                }

                if (result.Count >= minOverlaps) return (orientation, result.First().Item2 - result.First().Item1);
            }

            return null;
        }

        private int Commonality(Dictionary<long, List<Position3d>> p1, Dictionary<long, List<Position3d>> p2)
        {
            return p1.Keys.Intersect(p2.Keys).Sum(key => p1[key].Intersect(p2[key]).Count());
        }

        private List<Position3d> Orient(List<Position3d> p, Func<Position3d, Position3d> orientation)
        {
            return p.Select(orientation).ToList();
        }

        // The dictionary maps manhattandistance to points at that distance
        private List<Dictionary<long, List<Position3d>>> RelativePositions(List<Position3d> list)
        {
            // nested dictionary maps manhattan distance to number of other nodes at that distance
            var manhattanDistances = list.Select(_ => new Dictionary<long, List<Position3d>>()).ToList();
            foreach (var choice in list.WithIndices().Choose(2))
            {
                var (item, index) = choice[0];
                var (other, otherIndex) = choice[1];
                var m = item.ManhattanDistance(other);

                manhattanDistances[index].AddToList(m, other - item);
                manhattanDistances[otherIndex].AddToList(m, item - other);
            }

            return manhattanDistances;
        }

        public override long Part2(List<List<Position3d>> input)
        {
            throw new NotImplementedException();
        }
    }

}