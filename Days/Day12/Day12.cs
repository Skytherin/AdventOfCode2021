using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using AdventOfCode2021.Utils;
using FluentAssertions;
using JetBrains.Annotations;

namespace AdventOfCode2021.Days.Day12
{
    public class Day12 : IAdventOfCode
    {
        public void Run()
        {
            Part1();
            Part2();
        }

        private void Part1()
        {
            Do1(@"start-A
start-b
A-c
A-b
b-d
A-end
b-end").Should().Be(10);

            Do1(@"dc-end
HN-start
start-kj
dc-start
dc-HN
LN-dc
HN-end
kj-sa
kj-HN
kj-dc").Should().Be(19);

            Do1(@"fs-end
he-DX
fs-he
start-DX
pj-DX
end-zg
zg-sl
zg-pj
pj-he
RW-he
fs-DX
pj-RW
zg-RW
start-pj
he-WI
zg-he
pj-fs
start-RW").Should().Be(226);

            Do1(@"end-MY
MY-xc
ho-NF
start-ho
NF-xc
NF-yf
end-yf
xc-TP
MY-qo
yf-TP
dc-NF
dc-xc
start-dc
yf-MY
MY-ho
EM-uh
xc-yf
ho-dc
uh-NF
yf-ho
end-uh
start-NF").Should().Be(5076);
        }

        private int Do1(string s, bool revisitAllowed = false)
        {
            var inputs = StructuredRx.ParseLines<ConnectedNodes>(s);

            var caves = inputs.SelectMany(it => new[] { it.Node1, it.Node2 }).Distinct().ToList();

            var adjacencies = caves.ToDictionary(it => it, _ => new HashSet<string>());
            foreach (var input in inputs)
            {
                if (input.Node2 != "start") adjacencies[input.Node1].Add(input.Node2);
                if (input.Node1 != "start") adjacencies[input.Node2].Add(input.Node1);
            }

            var smallCaves = caves.Where(it => it.ToLower() == it).ToHashSet();
            var open = new Queue<(string Head, List<string> Path, bool RevisitAllowed)>();
            open.Enqueue(("start", new List<string>(), revisitAllowed));
            var found = 0;
            while (open.Count > 0)
            {
                var current = open.Dequeue();

                foreach (var adjacentCave in adjacencies[current.Head])
                {
                    var revisit = current.RevisitAllowed;
                    if (smallCaves.Contains(adjacentCave) && current.Path.Contains(adjacentCave))
                    {
                        if (!revisit) continue;
                        revisit = false;
                    }

                    if (adjacentCave == "end")
                    {
                        found += 1;
                    }
                    else
                    {
                        open.Enqueue((Head: adjacentCave, Path: current.Path.Append(adjacentCave).ToList(), RevisitAllowed: revisit));
                    }
                }
            }

            return found;
        }

        private int Do1__(string s, bool revisitAllowed = false)
        {
            var inputs = StructuredRx.ParseLines<ConnectedNodes>(s);

            var caves = inputs.SelectMany(it => new[] { it.Node1, it.Node2 }).Distinct().ToList();

            var adjacencies = caves.ToDictionary(it => it, _ => new HashSet<string>());
            foreach (var input in inputs)
            {
                adjacencies[input.Node1].Add(input.Node2);
                adjacencies[input.Node2].Add(input.Node1);
            }

            return adjacencies["start"].Sum(next => dfs(adjacencies, next, new List<string>(), revisitAllowed));
        }

        private int dfs(Dictionary<string, HashSet<string>> adjacencies, string cave, IReadOnlyList<string> visited, bool revisitAllowed)
        {
            if (cave == "start") return 0;
            if (cave == "end") return 1;
            var vallowed = revisitAllowed;
            if (cave == cave.ToLower())
            {
                var count = visited.Count(it => it == cave);
                if (count == 1 && !revisitAllowed) return 0;
                if (count == 1) vallowed = false;
                if (count >= 2) return 0;
                visited = visited.Append(cave).ToList();
            }

            return adjacencies[cave].Sum(next => dfs(adjacencies, next, visited, vallowed));
        }


        private void Part2()
        {
            Do1(@"start-A
start-b
A-c
A-b
b-d
A-end
b-end", true).Should().Be(36);

            Do1(@"dc-end
HN-start
start-kj
dc-start
dc-HN
LN-dc
HN-end
kj-sa
kj-HN
kj-dc", true).Should().Be(103);

            Do1(@"fs-end
he-DX
fs-he
start-DX
pj-DX
end-zg
zg-sl
zg-pj
pj-he
RW-he
fs-DX
pj-RW
zg-RW
start-pj
he-WI
zg-he
pj-fs
start-RW", true).Should().Be(3509);

            Do1(@"end-MY
MY-xc
ho-NF
start-ho
NF-xc
NF-yf
end-yf
xc-TP
MY-qo
yf-TP
dc-NF
dc-xc
start-dc
yf-MY
MY-ho
EM-uh
xc-yf
ho-dc
uh-NF
yf-ho
end-uh
start-NF", true).Should().Be(145643);
        }
    }

    public class ConnectedNodes
    {
        [RxFormat(After = "-")]
        public string Node1 { get; set; } = "";

        public string Node2 { get; set; } = "";
    }
}