namespace AdventOfCode2021.Utils
{
    public static class MoreMath
    {
        public static long Sign(long a) => a switch
        {
            < 0 => -1,
            > 0 => 1,
            _ => 0
        };
        public static long Abs(long a) => a < 0 ? -a : a;
    }
}