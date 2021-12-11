namespace AdventOfCode2021.Utils
{
    public class Vector
    {
        public static readonly Vector North = new(-1, 0);
        public static readonly Vector East = new(0, 1);
        public static readonly Vector South = new(1, 0);
        public static readonly Vector West = new(0, -1);

        public int dX { get; }
        public int dY { get; }

        public Vector(int dy, int dx)
        {
            dX = dx;
            dY = dy;
        }

        public static Vector operator +(Vector a, Vector b)
        {
            return new Vector(a.dY + b.dY, a.dX + b.dX);
        }

        public static Vector operator *(Vector a, int magnitude)
        {
            return new Vector(a.dY * magnitude, a.dX * magnitude);
        }

        public Vector RotateRight() => new(dX, -dY);
        public Vector RotateLeft() => new(-dX, dY);
    }
}