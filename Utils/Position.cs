using System;
using System.Collections.Generic;

namespace AdventOfCode2021.Utils
{
    public class Position
    {
        public static Position Zero = new(0, 0);
        public long X { get; }
        public long Y { get; }

        public Position(long y, long x)
        {
            X = x;
            Y = y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public override bool Equals(object? obj)
        {
            return obj is Position other && other.X == X && other.Y == Y;
        }

        public static Position operator +(Position p, Vector vector)
        {
            return new Position(p.Y + vector.dY, p.X + vector.dX);
        }

        public static bool operator ==(Position p, Position p2)
        {
            return p.Equals(p2);
        }

        public static bool operator !=(Position p, Position p2)
        {
            return !(p == p2);
        }

        public static Position operator -(Position p, Position p2)
        {
            return new Position(p.Y - p2.Y, p.X - p2.X);
        }

        public long ManhattanDistance()
        {
            return Math.Abs(X) + Math.Abs(Y);
        }

        public long ManhattanDistance(Position other)
        {
            return (this - other).ManhattanDistance();
        }

        public IEnumerable<Position> Orthogonals()
        {
            yield return North;
            yield return South;
            yield return East;
            yield return West;
        }

        public Position North => this + Vector.North;
        public Position South => this + Vector.South;
        public Position East => this + Vector.East;
        public Position West => this + Vector.West;

        public override string ToString()
        {
            return $"({Y},{X})";
        }
    }
}