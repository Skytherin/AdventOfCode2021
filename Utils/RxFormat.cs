using System;

namespace AdventOfCode2021.Utils
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RxFormat : Attribute
    {
        public string? After { get; set; }

        public string? Before { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class RxAlternate : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class RxRepeat : Attribute
    {
        public readonly int Min;
        public readonly int Max;

        public RxRepeat(int min = 0, int max = int.MaxValue)
        {
            Min = min;
            Max = max;
        }
    }
}