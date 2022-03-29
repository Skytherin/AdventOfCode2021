using System;
using System.Collections.Generic;
using System.Linq;
using AdventOfCode2021.Utils;
using FluentAssertions;
using JetBrains.Annotations;

namespace AdventOfCode2021.Days.Day23
{
    [UsedImplicitly]
    public class Day23: IAdventOfCode
    {
        public void Run()
        {
            var example = new Burrow(2, B, A, C, D, B, C, D, A);

            var input = new Burrow(2, A, D, C, A, B, D, C, B);

            Part1(example).Should().Be(12521);
            Part1(input).Should().Be(16300);
            Part1(new Burrow(4, B, D, D, A, C, C, B, D, B, B, A, C, D, A, C, A)).Should().Be(44169);
            Part1(new Burrow(4, A, D, D, D, C, C, B, A, B, B, A, D, C, A, C, B)).Should().Be(48676L);
        }

        public long Part1(Burrow input)
        {
            var open = new PriorityQueue<Node>(node => node.TotalMoveCost + node.Burrow.TotalPotentialEnergy());
            open.Enqueue(new(input, 0));
            var visited = new Dictionary<Burrow, long>()
            {
                { input, 0 }
            };

            //var outputTemp = 0L;
            while (open.TryDequeue(out var current))
            {
                if (visited.TryGetValue(current.Burrow, out var temp) && temp < current.TotalMoveCost) continue;

                //if (current.TotalMoveCost >= outputTemp + 1000)
                //{
                //    outputTemp = current.TotalMoveCost;
                //    Console.WriteLine($"{current}, {current.Burrow.TotalPotentialEnergy()}");
                //}
                foreach (var move in Neighbors(current.Burrow))
                {
                    var totalEnergy = current.TotalMoveCost + move.MoveCost;
                    if (move.Burrow.TotalPotentialEnergy() == 0)
                    {
                        return totalEnergy;
                    }

                    if (visited.TryGetValue(move.Burrow, out var moveTemp) && moveTemp <= totalEnergy) continue;
                    visited[move.Burrow] = totalEnergy;

                    open.Enqueue(new Node(move.Burrow, totalEnergy));
                }
            }

            throw new ApplicationException();
        }

        public IEnumerable<(Burrow Burrow, long MoveCost)> Neighbors(Burrow burrow)
        {
            foreach (var (p1, p2) in Moves(burrow))
                yield return (burrow.Move(p1, p2), p1.ManhattanDistance(p2) * burrow.Amphipods[p1].MoveCost);
        }

        public IEnumerable<(Position, Position)> Moves(Burrow burrow)
        {
            foreach (var (position, a) in burrow.Amphipods)
            {
                var (x, y) = (position.X, position.Y);

                if (burrow.InPlace(position, a)) continue;

                // Amphipods can leave a room into the hallway
                if (IsRoom(position))
                {
                    if (EnumerableExtensions.Range2(1, y - 1)
                        .Any(y2 => burrow.ColorAt(y2, x) != null)) // blocked
                    {
                        continue;
                    }
                    var legalHallwayPositions = new[] { 0, 1, 3, 5, 7, 9, 10 };
                    foreach (var x2 in legalHallwayPositions.TakeWhile(x2 => x2 < x).Reverse())
                    {
                        if (burrow.ColorAt(0, x2) != null) break;
                        yield return (position, new Position(0, x2));
                    }
                    foreach (var x2 in legalHallwayPositions.SkipWhile(x2 => x2 < x))
                    {
                        if (burrow.ColorAt(0, x2) != null) break;
                        yield return (position, new Position(0, x2));
                    }

                    continue;
                }

                // Amphipod is in hallway, will only move to destination room
                var dx = a.DestinationX;

                if (EnumerableExtensions.Range2(x + 1, dx).Any(x2 => burrow.ColorAt(0, x2) != null)) continue;
                if (EnumerableExtensions.Range2(dx, x - 1).Any(x2 => burrow.ColorAt(0, x2) != null)) continue;

                var dy = burrow.RoomCapacity;
                var blocked = false;
                for (var y2 = 1; y2 <= burrow.RoomCapacity; y2++)
                {
                    if (burrow.ColorAt(y2, dx) == null)
                    {
                        dy = y2;
                    }
                    else if (burrow.ColorAt(y2, dx) != a.Color)
                    {
                        blocked = true;
                        break;
                    }
                }

                if (!blocked)
                {
                    yield return (position, new Position(dy, dx));
                }
            }
        }

        private bool IsRoom(Position position)
        {
            return new[] { 2L, 4, 6, 8 }.Contains(position.X);
        }

        private static Amphipod A => new('A', 1, 2);
        private static Amphipod B => new('B', 10, 4);
        private static Amphipod C => new('C', 100, 6);
        private static Amphipod D => new('D', 1000, 8);
    }

    public record Node(Burrow Burrow, long TotalMoveCost);

    public class Burrow
    {
        public readonly IReadOnlyDictionary<Position, Amphipod> Amphipods;
        public readonly int RoomCapacity;

        public Burrow(int roomCapacity, params Amphipod[] amphipods)
        {
            RoomCapacity = roomCapacity;
            amphipods.Should().HaveCount(roomCapacity * 4);
            amphipods.Where(it => it.Color == 'A').Should().HaveCount(roomCapacity);
            amphipods.Where(it => it.Color == 'B').Should().HaveCount(roomCapacity);
            amphipods.Where(it => it.Color == 'C').Should().HaveCount(roomCapacity);
            amphipods.Where(it => it.Color == 'D').Should().HaveCount(roomCapacity);
            Amphipods = new List<int> { 2, 4, 6, 8 }.Select(x => Enumerable.Range(1, roomCapacity).Select(y => new Position(y, x)))
                .Flatten().Zip(amphipods).ToDictionary(it => it.First, it => it.Second);
        }

        private Burrow(int roomCapacity, IReadOnlyDictionary<Position, Amphipod> d)
        {
            RoomCapacity = roomCapacity;
            Amphipods = d;
        }

        public override bool Equals(object? other)
        {
            if (other is not Burrow otherBurrow) return false;

            return Amphipods.All(item =>
                otherBurrow.Amphipods.TryGetValue(item.Key, out var otherA) && item.Value.Color == otherA.Color);
        }

        public override int GetHashCode()
        {
            return Amphipods.OrderBy(it => it.Key.Y).ThenBy(it => it.Key.X)
                .Aggregate(0, (current, item) => HashCode.Combine(current, item.Key, item.Value));
        }

        public char? ColorAt(long y, long x)
        {
            if (Amphipods.TryGetValue(new Position(y, x), out var temp)) return temp.Color;
            return null;
        }

        public Burrow Move(Position p1, Position p2)
        {
            var d = Amphipods.ToDictionary(it => it.Key, it => it.Value);
            d[p2] = d[p1];
            d.Remove(p1);
            return new Burrow(RoomCapacity, d);
        }

        public override string ToString()
        {
            return Amphipods.OrderBy(it => it.Key.X).ThenBy(it => it.Key.Y)
                .Select(it => $"({it.Key}, {it.Value.Color})").Join(", ");
        }

        public long TotalPotentialEnergy()
        {
            return Amphipods.Aggregate(0L, (current, item) => current + PotentialEnergy(item.Key, item.Value));
        }

        public bool InPlace(Position p, Amphipod a)
        {
            if (p.X != a.DestinationX) return false;
            foreach (var y in EnumerableExtensions.Range2(p.Y + 1, RoomCapacity))
            {
                if (ColorAt(y, a.DestinationX) != a.Color)
                {
                    return false;
                }
            }

            return true;
        }

        private long PotentialEnergy(Position p, Amphipod a)
        {
            if (InPlace(p, a)) return 0;

            if (p.X == a.DestinationX)
            {
                return (p.Y + 3) * a.MoveCost;
            }

            return (p.Y + Math.Abs(p.X - a.DestinationX) + 1) * a.MoveCost;
        }
    }

    public record Amphipod(char Color, long MoveCost, int DestinationX);
}