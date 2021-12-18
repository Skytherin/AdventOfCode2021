using System;
using System.Collections.Generic;

namespace AdventOfCode2021.Utils
{
    public interface IEither<T1, T2>
    {
        T3 Map<T3>(Func<T1, T3> map1, Func<T2, T3> map2);
    }

    public class FirstEither<T1, T2>: IEither<T1, T2>
    {
        public readonly T1 Value;

        public FirstEither(T1 value)
        {
            Value = value;
        }

        public T3 Map<T3>(Func<T1, T3> map1, Func<T2, T3> map2)
        {
            return map1(Value);
        }
    }

    public class SecondEither<T1, T2> : IEither<T1, T2>
    {
        public readonly T2 Value;

        public SecondEither(T2 value)
        {
            Value = value;
        }

        public T3 Map<T3>(Func<T1, T3> map1, Func<T2, T3> map2)
        {
            return map2(Value);
        }
    }
}