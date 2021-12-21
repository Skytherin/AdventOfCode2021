using System.Collections.Generic;
using System.Linq;
using AdventOfCode2021.Utils;
using JetBrains.Annotations;

namespace AdventOfCode2021.Days.Day14
{
    [UsedImplicitly]
    public class Day14 : AdventOfCode<Day14Input>
    {
        public override Day14Input Parse(string input)
        {
            var p = input.Paragraphs().ToList();

            var template = p[0][0];

            var pairs = p[1].Select(StructuredRx.Parse<PairInsertion>);

            return new(template, pairs.ToList());
        }

        [TestCase(Input.Example, 1588)]
        [TestCase(Input.File, 2345)]
        public override long Part1(Day14Input input)
        {
            return Polymerize(input, 10);
        }

        [TestCase(Input.Example, 2188189693529)]
        [TestCase(Input.File, 2432786807053)]
        public override long Part2(Day14Input input)
        {
            return Polymerize(input, 40);
        }

        private long Polymerize(Day14Input input, int reps)
        {
            var cache = new Dictionary<(char, char, int), IReadOnlyDictionary<char, long>>();
            var d = input.PairInsertions.ToDictionary(it => (it.Pair[0], it.Pair[1]), it => it.InsertionCharacter[0]);
            
            var counts = input.Template.GroupBy(it => it).ToDictionary(it => it.Key, it => it.LongCount()) as IReadOnlyDictionary<char, long>;

            counts = input.Template.Windows(2).Aggregate(counts,
                (current, window) => CombineCounts(current, GetAllInsertionsForPair(window[0], window[1], reps, d, cache)));

            var smallest = counts.MinBy(it => it.Value).Value;
            var largest = counts.MaxBy(it => it.Value).Value;

            return largest - smallest;
        }

        private IReadOnlyDictionary<char, long> CombineCounts(IReadOnlyDictionary<char, long> destination, IReadOnlyDictionary<char, long> source)
        {
            return destination.Keys.Union(source.Keys).ToDictionary(it => it,
                it => destination.GetValueOrDefault(it) + source.GetValueOrDefault(it));
        }

        private IReadOnlyDictionary<char, long> GetAllInsertionsForPair(char left, char right, int reps, 
            IReadOnlyDictionary<(char, char), char> insertionMap,
            Dictionary<(char, char, int), IReadOnlyDictionary<char, long>> cache)
        {
            if (reps == 0) return new Dictionary<char, long>();
            if (!insertionMap.TryGetValue((left, right), out var insertion)) return new Dictionary<char, long>();
            if (cache.TryGetValue((left, right, reps), out var value)) return value;
            var result = new Dictionary<char, long>
            {
                {insertion, 1}
            } as IReadOnlyDictionary<char, long>;
            result = CombineCounts(result, GetAllInsertionsForPair(left, insertion, reps - 1, insertionMap, cache));
            result = CombineCounts(result, GetAllInsertionsForPair(insertion, right, reps - 1, insertionMap, cache));
            cache[(left, right, reps)] = result;
            return result;
        }
    }

    public record Day14Input(string Template, IReadOnlyList<PairInsertion> PairInsertions);

    public class PairInsertion
    {
        public string Pair { get; set; } = "";

        [RxFormat(Before = "->")]
        public string InsertionCharacter { get; set; } = "";
    }
}