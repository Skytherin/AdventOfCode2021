using System;
using System.Collections.Generic;
using System.Linq;
using AdventOfCode2021.Utils;
using FluentAssertions;
using FluentAssertions.Execution;

namespace AdventOfCode2021.Days.Day24
{
    public class Day24: IAdventOfCode
    {
        public void RunMachine()
        {
            Console.WriteLine();

            var machine = new AluMachine();

//            machine.Process(Parse(@"inp x
//mul x -1"), new List<long>{7L});

            //machine.Process(Parse(@"inp z
            //    inp x
            //    mul z 3
            //    eql z x"), new List<long> { 1, 3 });

            //            var result = new AluMachine().Process(Parse(@"inp w
            //add z w
            //mod z 2
            //div w 2
            //add y w
            //mod y 2
            //div w 2
            //add x w
            //mod x 2
            //div w 2
            //mod w 2"), new[] { 10L }.ToQueue());

            var input = Parse(this.File());

            machine.Process(input,
                "2222_2222_2222_22".Where(c => c != '_').Select(it => Convert.ToInt64($"{it}")).ToList());

            Console.WriteLine($"Z = {machine.Equations[3]}");
            foreach (var item in machine.Variables)
            {
                Console.WriteLine(item);
            }


        }

        private List<AluInstruction> Parse(string s)
        {
            return s.Lines()
                .Select(line =>
                {
                    var split = line.Split(" ");
                    var type = split[0].ToLower() switch
                    {
                        "inp" => AluInstructionE.Inp,
                        "add" => AluInstructionE.Add,
                        "sub" => AluInstructionE.Sub,
                        "div" => AluInstructionE.Div,
                        "mul" => AluInstructionE.Mul,
                        "mod" => AluInstructionE.Mod,
                        "eql" => AluInstructionE.Eql,
                        _ => throw new ApplicationException()
                    };

                    var a = split[1].ToLower().Single();

                    if (split.Length < 3)
                    {
                        return new AluInstruction(AluInstructionE.Inp, a, 0);
                    }

                    if (new[] { "w", "x", "y", "z"}.Contains(split[2].ToLower()))
                    {
                        return new AluInstruction(type, a, split[2].ToLower()[0]);
                    }

                    return new AluInstruction(type, a, Convert.ToInt64(split[2]));
                })
                .ToList();
        }

        public void Run()
        {
            //Console.WriteLine();
            //foreach(var i in new[]
            //        {
            //            1111_1111_1111_11,
            //            2222_2222_2222_22,
            //            3333_3333_3333_33,
            //            1234_5678_4566_22,
            //            342354756___34534,
            //            9999_9999_9999_99,
            //            823834864___13498
            //        })
            //{
            //    var m = new AluMachine();
            //    var args = i.ToString().Select(it => Convert.ToInt64($"{it}")).ToList();
            //    m.Process(Parse(this.File()), args);
            //    var d = args.WithIndices().Aggregate(0L, (i1, tuple) => Score(tuple.Index, (int)tuple.Value, i1));
            //    m.Registers[3].Should().Be(d);
            //}

            //RunMachine();
            var temp = Lowest(0, 0);
            Console.WriteLine(temp);
        }

        (long,long,long)[] Coefficients = new[]
        {
            (0L,0L,0L),
            (1L,10,10),
            (1L,13,2), // 2
            (26L, -10, 5),
            (1L,11,6), // 4,
            (1L, 11, 0),
            (1L,12,16), // 6,
            (26L, -11, 12),
            (26L, -7, 15), // 8
            (1L, 13, 7),
            (26L, -13, 6), // 10
            (26L, 0 , 5), // 11
            (26L, -11, 6),
            (26L, 0, 15)
        };

        public long Score(int digit, int input, long previousScore)
        {
            if (digit == 0)
            {
                return input + 1;
            }

            var coeff = Coefficients[digit];
            var q = NotEql((previousScore % 26) + coeff.Item2, input);
            var score = (previousScore / coeff.Item1) * (25 * q + 1);
            score += (input + coeff.Item3) * q;
            return score;
        }

        private readonly Dictionary<(int, long), long?> Cache = new ();

        public long? Highest(int digit, long previousScore)
        {
            if (Cache.TryGetValue((digit, previousScore), out var temp)) return temp;
            long? result = null;
            for (var input = 9; input >= 1; input--)
            {
                var score = Score(digit, input, previousScore);

                if (digit == 13)
                {
                    if (score == 0)
                    {
                        result = input;
                        break;
                    }

                    continue;
                }

                var highest = Highest(digit + 1, score);
                if (highest != null)
                {
                    result = 1L * input * Pow10(13 - digit) + highest;
                    break;
                }
            }

            Cache[(digit, previousScore)] = result;
            return result;
        }

        private long CacheHit = 0;
        private long Calls = 0;
        public long? Lowest(int digit, long previousScore)
        {
            Calls += 1;
            if (Cache.TryGetValue((digit, previousScore), out var temp))
            {
                CacheHit += 1;
                return temp;
            }
            long? result = null;
            for (var input = 1; input <= 9; input++)
            {
                var score = Score(digit, input, previousScore);

                if (digit == 13)
                {
                    if (score == 0)
                    {
                        result = input;
                        break;
                    }

                    continue;
                }

                var highest = Lowest(digit + 1, score);
                if (highest != null)
                {
                    result = 1L * input * Pow10(13 - digit) + highest;
                    break;
                }
            }

            Cache[(digit, previousScore)] = result;
            return result;
        }

        private long Pow10(long power)
        {
            var result = 1L;
            for (var t = 0; t < power; t++) result *= 10;
            return result;
        }

        private int NotEql(long a, int b)
        {
            return a != b ? 1 : 0;
        }
    }

    public class AluMachine
    {
        public long[] Registers { get; } = { 0L, 0, 0, 0 };
        public string[] Equations { get; } = { "0", "0", "0", "0" };
        private int InputIndex = 0;
        public List<(string, string)> Variables = new();

        public (long W, long X, long Y, long Z) Process(List<AluInstruction> instructions, List<long> input)
        {
            foreach (var i in Enumerable.Range(0, 4))
            {
                Registers[i] = 0;
                Equations[i] = "0";
            }

            InputIndex = 0;

            foreach (var instruction in instructions)
            {
                if (instruction.Type == AluInstructionE.Inp && instruction != instructions.First())
                {
                    AssignVariable(3);
                }
                Process1(instruction, input);

            }

            return (Registers[0], Registers[1], Registers[2], Registers[3]);
        }

        private void AssignVariable(int register)
        {
            var name = $"Var{Variables.Count}";
            Variables.Add((name, Equations[register]));
            Equations[register] = name;
        }

        private void Process1(AluInstruction instruction, List<long> input)
        {
            var aIsVar = Equations[RegMap[instruction.A]].Contains("Inp") || Equations[RegMap[instruction.A]].Contains("Var");
            var bIsVar = instruction.B is FirstEither<char, long> tempB &&
                         (Equations[RegMap[tempB.Value]].Contains("Inp") || Equations[RegMap[instruction.A]].Contains("Var"));

            var valueA = GetValue(instruction.A);
            var valueB = GetValue(instruction.B);

            var a = instruction.A;
            var b = instruction.B.Map(c => c, _ => (char?)null);

            switch (instruction.Type)
            {
                case AluInstructionE.Inp:
                    var index = InputIndex++;
                    Equations[RegMap[a]] = $"Inp[{index}]";
                    SetValue(instruction.A, input[index]);
                    return;
                case AluInstructionE.Add:
                {
                    SetValue(instruction.A, valueA + valueB);
                    if (!bIsVar && valueB == 0)
                    {
                        return;
                    }
                    
                    if (!aIsVar && valueA == 0)
                    {
                        UpdateEquation(a, GetEquation(instruction.B));
                        return;
                    }

                    if (!aIsVar && !bIsVar)
                    {
                        UpdateEquation(a, $"{valueA + valueB}");
                        return;
                    }
                    UpdateEquation(a, '+', instruction.B);
                    break;
                }
                case AluInstructionE.Sub:
                {
                    SetValue(instruction.A, valueA - valueB);
                    if (!bIsVar && valueB == 0) return;
                    if (!aIsVar && !bIsVar)
                    {
                        UpdateEquation(a, $"{valueA - valueB}");
                        return;
                    }
                    UpdateEquation(a, '-', instruction.B);
                    
                    break;
                }
                case AluInstructionE.Mul:
                {
                    SetValue(instruction.A, valueA * valueB);
                    if (!bIsVar && valueB == 1) return;
                    if ((!aIsVar && valueA == 0) || (!bIsVar && valueB == 0))
                    {
                        UpdateEquation(a, "0");
                        return;
                    }
                    if (!aIsVar && valueA == 1)
                    {
                        UpdateEquation(a, GetEquation(instruction.B));
                        return;
                    }
                    if (!aIsVar && !bIsVar)
                    {
                        UpdateEquation(a, $"{valueA * valueB}");
                        return;
                    }
                    UpdateEquation(a, '*', instruction.B);
                    
                    break;
                }
                case AluInstructionE.Div:
                {
                    SetValue(instruction.A, valueA / valueB);
                    if (!bIsVar && valueB == 1) return;
                    if (!aIsVar && !bIsVar)
                    {
                        UpdateEquation(a, $"{valueA / valueB}");
                        return;
                    }
                    UpdateEquation(a, '/', instruction.B);
                    
                    break;
                }
                case AluInstructionE.Mod:
                {
                    SetValue(instruction.A, valueA % valueB);
                    if (!aIsVar && valueA == 0) return;
                    if (!aIsVar && !bIsVar)
                    {
                        UpdateEquation(a, $"{valueA % valueB}");
                        return;
                    }
                    UpdateEquation(a, '%', instruction.B);
                    break;
                }
                case AluInstructionE.Eql:
                {
                    SetValue(instruction.A, valueA == valueB ? 1 : 0);
                    if (!aIsVar && !bIsVar)
                    {
                        UpdateEquation(instruction.A, valueA == valueB ? "1" : "0");
                    }
                    else if (!bIsVar && valueB == 0 && GetEquation(a).StartsWith("Eql"))
                    {
                        UpdateEquation(a, $"Not{GetEquation(a)}");
                    }
                    else if (b is {})
                    {
                        UpdateEquation(instruction.A, $"Eql({Equations[RegMap[instruction.A]]}, {Equations[RegMap[b.Value]]})");
                    }
                    else 
                    {
                        UpdateEquation(instruction.A, $"Eql({Equations[RegMap[instruction.A]]}, {valueB})");
                    }
                    
                    return;
                }
                default:
                    throw new ApplicationException();
            }
        }

        private string GetEquation(IEither<char, long> b)
        {
            return b.Map(c => Equations[RegMap[c]], bl => bl.ToString());
        }

        private string GetEquation(char register)
        {
            return Equations[RegMap[register]];
        }

        private Dictionary<char, char> LastOp = new();

        private void UpdateEquation(char register, char op, IEither<char, long> b)
        {
            var s1 = Equations[RegMap[register]];
            var s2 = GetEquation(b);
            if (new[] { '*', '/', '%' }.Contains(op))
            {
                s1 = Parens(s1);
                s2 = Parens(s2);
            }

            var s = s1 + $" {op} " + s2;

            Equations[RegMap[register]] = s;
        }

        private string Parens(string s)
        {
            var depth = 0;
            foreach (var c in s)
            {
                if (c == '(') depth += 1;
                else if (c == ')') depth -= 1;
                else if (depth == 0 && (c == '+' || c == '-')) return $"({s})";
            }

            return s;
        }

        private void UpdateEquation(char register, string s)
        {
            Equations[RegMap[register]] = s;
        }

        private long GetValue(IEither<char, long> b)
        {
            return b.Map(c => GetValue(c), l => l);
        }

        private Dictionary<char, int> RegMap = new()
        {
            { 'w', 0 },
            { 'x', 1 },
            { 'y', 2 },
            { 'z', 3 }
        };

        private long GetValue(char c)
        {
            return Registers[RegMap[c]];
        }

        private void SetValue(char c, long l)
        {
            Registers[RegMap[c]] = l;
        }
    }

    public class AluInstruction
    {
        public AluInstructionE Type { get; }
        public char A { get; } 
        public IEither<char, long> B { get; }

        public AluInstruction(AluInstructionE type, char a, char b)
        {
            Type = type;
            A = a;
            B = new FirstEither<char, long>(b);
        }

        public AluInstruction(AluInstructionE type, char a, long b)
        {
            Type = type;
            A = a;
            B = new SecondEither<char, long>(b);
        }

        public override string ToString()
        {
            if (Type == AluInstructionE.Inp) return $"Inp {A}";
            var b = B.Map(c => $"{c}", l => l.ToString());
            return $"{Type} {A} {b}";
        }
    }

    public enum AluInstructionE
    {
        Inp,
        Add,
        Sub,
        Mul,
        Div,
        Mod,
        Eql
    }
}