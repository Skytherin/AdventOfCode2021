using System;
using System.Collections.Generic;
using System.Linq;
using AdventOfCode2021.Utils;
using FluentAssertions;

namespace AdventOfCode2021.Days.Day25;

public class Day25: IAdventOfCode
{
    public void Run()
    {
        Part1(Parse(this.Example())).Should().Be(58);
        Part1(Parse(this.File())).Should().Be(518);
    }

    private long Part1((Dictionary<Position, char> Map, int Width, int Height) initial)
    {
        var current = initial.Map;
        var count = 0;
        var moved = true;
        while (moved)
        {
            moved = false;
            var copy = current.Select(it =>
            {
                if (it.Value == 'v') return (it.Key, it.Value);
                var destination = it.Key.East;
                if (destination.X >= initial.Width) destination = new Position(destination.Y, 0);
                if (!current.ContainsKey(destination))
                {
                    moved = true;
                    return (destination, it.Value);
                }

                return (it.Key, it.Value);
            }).ToDictionary(it => it.Item1, it => it.Item2);

            current = copy.Select(it =>
            {
                if (it.Value == '>') return (it.Key, it.Value);
                var destination = it.Key.South;
                if (destination.Y >= initial.Height) destination = new Position(0, destination.X);
                if (!copy.ContainsKey(destination))
                {
                    moved = true;
                    return (destination, it.Value);
                }

                return (it.Key, it.Value);
            }).ToDictionary(it => it.Item1, it => it.Item2);
            count++;
        }

        return count;
    }

    private (Dictionary<Position, char> Map, int Width, int Height) Parse(string s)
    {
        var map = s.Lines().WithIndices()
            .SelectMany(row => row.Value.WithIndices().Where(col => col.Value != '.').Select(
                col => (new Position(row.Index, col.Index), col.Value))).ToDictionary(it => it.Item1, it => it.Item2);

        var height = s.Lines().Count;
        var width = s.Lines().First().Length;

        return (map, width, height);
    }
}