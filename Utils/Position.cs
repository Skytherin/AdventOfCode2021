using System;
using System.Collections.Generic;

namespace AdventOfCode2015.Utils
{
    public class Position
    {
        public static Position Zero = new Position(0, 0);
        public int X { get; }
        public int Y { get; }

        public Position(int x, int y)
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
            return new Position(p.X + vector.dX, p.Y + vector.dY);
        }

        public long ManhattanDistance()
        {
            return Math.Abs(X) + Math.Abs(Y);
        }

        public IEnumerable<Position> Orthogonal()
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
    }
}