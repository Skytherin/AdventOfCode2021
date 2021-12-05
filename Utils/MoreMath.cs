namespace AdventOfCode2021.Utils
{
    public static class MoreMath
    {
        public static int Sign(int a) => a switch
        {
            < 0 => -1,
            > 0 => 1,
            _ => 0
        };
        public static int Abs(int a) => a < 0 ? -a : a;
    }
}