using System.Collections.Generic;

namespace AdventOfCode2021.Utils
{
    public static class DictionaryExtensions
    {
        public static void AddToList<TKey, TValue>(this IDictionary<TKey, List<TValue>> self, TKey key, TValue value)
        {
            if (self.TryGetValue(key, out var list))
            {
                list.Add(value);
            }
            else
            {
                self[key] = new() { value };
            }
        }
    }
}