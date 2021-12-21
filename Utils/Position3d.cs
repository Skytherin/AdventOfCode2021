using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode2021.Utils
{
    public class Position3d
    {
        public static Position3d Zero = new(0, 0, 0);
        public long X { get; }
        public long Y { get; }
        public long Z { get; }

        public Position3d(long y, long x, long z = 0)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Position3d(IEnumerable<long> input)
        {
            var list = input.Take(3).ToList();
            X = list.Skip(1).FirstOrDefault();
            Y = list.Skip(0).FirstOrDefault();
            Z = list.Skip(2).FirstOrDefault();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        public override bool Equals(object? obj)
        {
            return obj is Position3d other && other.X == X && other.Y == Y && other.Z == Z;
        }

        public static bool operator ==(Position3d p, Position3d p2)
        {
            return p.Equals(p2);
        }

        public static bool operator !=(Position3d p, Position3d p2)
        {
            return !(p == p2);
        }

        public static Position3d operator -(Position3d p, Position3d p2)
        {
            return new Position3d(p.Y - p2.Y, p.X - p2.X, p.Z - p2.Z);
        }

        public static Position3d operator -(Position3d p)
        {
            return new Position3d(-p.Y, -p.X, -p.Z);
        }

        public static Position3d operator +(Position3d p, Position3d p2)
        {
            return new Position3d(p.Y + p2.Y, p.X + p2.X, p.Z + p2.Z);
        }

        public long ManhattanDistance()
        {
            return Math.Abs(X) + Math.Abs(Y) + Math.Abs(Z);
        }

        public long ManhattanDistance(Position3d other)
        {
            return (this - other).ManhattanDistance();
        }

        public override string ToString()
        {
            return $"({Y},{X},{Z})";
        }

        public Position3d RollUp() => new Position3d(-Z, X, Y);

        public Position3d RollLeft() => new Position3d(X, -Y, Z);

        public Position3d Rotate() => new Position3d(Y, Z, -X);
    }
}