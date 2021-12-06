using System;
using System.IO;
using System.Reflection;
using FluentAssertions;
using JetBrains.Annotations;

namespace AdventOfCode2021.Utils
{
    public interface IAdventOfCode
    {
        void Run();
    }

    public abstract class AdventOfCode<T>: IAdventOfCode
    {
        public void Run()
        {
            var example = Parse(Example);
            var classname = GetType().Name;
            var file = Parse(File.ReadAllText($"Days/{classname}/Input.txt"));
            var testCases = GetType().GetMethod("Part1")!.GetCustomAttributes<TestCaseAttribute>();
            foreach (var testCase in testCases)
            {
                Part1(testCase.Input == Input.Example ? example : file).Should().Be(testCase.Expected);
            }
            testCases = GetType().GetMethod("Part2")!.GetCustomAttributes<TestCaseAttribute>();
            foreach (var testCase in testCases)
            {
                Part2(testCase.Input == Input.Example ? example : file).Should().Be(testCase.Expected);
            }
        }

        public abstract T Parse(string input);

        public abstract long Part1(T input);
        public abstract long Part2(T input);

        public virtual string Example => "";
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class TestCaseAttribute: Attribute
    {
        public Input Input { get; }
        public long Expected { get; }

        public TestCaseAttribute(Input input, long expected)
        {
            Input = input;
            Expected = expected;
        }
    }

    public enum Input
    {
        Example,
        File
    }
}