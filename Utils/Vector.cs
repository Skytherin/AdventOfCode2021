namespace AdventOfCode2015.Utils
{
    public class Vector
    {
        public static readonly Vector North = new Vector(0, -1);
        public static readonly Vector East = new Vector(1, 0);
        public static readonly Vector South = new Vector(0, +1);
        public static readonly Vector West = new Vector(-1, 0);

        public int dX { get; }
        public int dY { get; }

        public Vector(int dx, int dy)
        {
            dX = dx;
            dY = dy;
        }

        public static Vector operator +(Vector a, Vector b)
        {
            return new Vector(a.dX + b.dX, a.dY + b.dY);
        }

        public static Vector operator *(Vector a, int magnitude)
        {
            return new Vector(a.dX * magnitude, a.dY * magnitude);
        }
    }
}