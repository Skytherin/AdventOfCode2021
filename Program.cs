using System;
using System.Linq;
using System.Reflection;
using AdventOfCode2021.Utils;

namespace AdventOfCode2021
{
    class Program
    {
        static void Main()
        {
            var regression = false;

            var days = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(type => !type.IsAbstract)
                .Where(type => type.IsClass)
                .Where(type => type.IsAssignableTo(typeof(IAdventOfCode)))
                .Select(type => (type, StructuredRx.ParseOrDefault<DayClass>(type.Name)))
                .Where(it => it.Item2 != null)
                .OrderBy(it => it.Item2!.DayNumber)
                .ToList();

            if (!regression)
            {
                days = EnumerableExtensions.ListFromItem(days.Last());
            }

            foreach (var day in days)
            {
                Console.Write(day.type.Name);
                var start = DateTime.Now;
                var instance = (IAdventOfCode)day.type.GetConstructor(new Type[] { })!.Invoke(new object?[] { });
                instance.Run();
                var stop = DateTime.Now;
                Console.WriteLine($"  {(stop - start).TotalSeconds:N3}s");
            }
        }
    }

    public class DayClass
    {
        [RxFormat(Before = "Day")]
        public int DayNumber { get; set; }
    }
}