using System.Collections.Generic;
using System.Linq;
using AdventOfCode2021.Utils;
using FluentAssertions;
using JetBrains.Annotations;

namespace AdventOfCode2021.Days.Day16
{
    [UsedImplicitly]
    public class Day16 : IAdventOfCode
    {
        private const string Input = @"6051639005B56008C1D9BB3CC9DAD5BE97A4A9104700AE76E672DC95AAE91425EF6AD8BA5591C00F92073004AC0171007E0BC248BE0008645982B1CA680A7A0CC60096802723C94C265E5B9699E7E94D6070C016958F99AC015100760B45884600087C6E88B091C014959C83E740440209FC89C2896A50765A59CE299F3640D300827902547661964D2239180393AF92A8B28F4401BCC8ED52C01591D7E9D2591D7E9D273005A5D127C99802C095B044D5A19A73DC0E9C553004F000DE953588129E372008F2C0169FDB44FA6C9219803E00085C378891F00010E8FF1AE398803D1BE25C743005A6477801F59CC4FA1F3989F420C0149ED9CF006A000084C5386D1F4401F87310E313804D33B4095AFBED32ABF2CA28007DC9D3D713300524BCA940097CA8A4AF9F4C00F9B6D00088654867A7BC8BCA4829402F9D6895B2E4DF7E373189D9BE6BF86B200B7E3C68021331CD4AE6639A974232008E663C3FE00A4E0949124ED69087A848002749002151561F45B3007218C7A8FE600FC228D50B8C01097EEDD7001CF9DE5C0E62DEB089805330ED30CD3C0D3A3F367A40147E8023221F221531C9681100C717002100B36002A19809D15003900892601F950073630024805F400150D400A70028C00F5002C00252600698400A700326C0E44590039687B313BF669F35C9EF974396EF0A647533F2011B340151007637C46860200D43085712A7E4FE60086003E5234B5A56129C91FC93F1802F12EC01292BD754BCED27B92BD754BCED27B100264C4C40109D578CA600AC9AB5802B238E67495391D5CFC402E8B325C1E86F266F250B77ECC600BE006EE00085C7E8DF044001088E31420BCB08A003A72BF87D7A36C994CE76545030047801539F649BF4DEA52CBCA00B4EF3DE9B9CFEE379F14608";

        public void Run()
        {
            ParsePacket("8A004A801A8002F478").VersionSum.Should().Be(16);
            ParsePacket("620080001611562C8802118E34").VersionSum.Should().Be(12);
            ParsePacket("C0015000016115A2E0802F182340").VersionSum.Should().Be(23);
            ParsePacket("A0016C880162017C3686B18A3D4780").VersionSum.Should().Be(31);
            ParsePacket(Input).VersionSum.Should().Be(1038);

            ParsePacket(Input).Value.Should().Be(246761930504L);
        }

        private Packet ParsePacket(string s)
        {
            var m = new Dictionary<char, List<bool>>
            {
                { '0', new List<bool> { false, false, false, false } },
                { '1', new List<bool> { false, false, false, true } },
                { '2', new List<bool> { false, false, true, false } },
                { '3', new List<bool> { false, false, true, true } },
                { '4', new List<bool> { false, true, false, false } },
                { '5', new List<bool> { false, true, false, true } },
                { '6', new List<bool> { false, true, true, false } },
                { '7', new List<bool> { false, true, true, true } },
                { '8', new List<bool> { true, false, false, false } },
                { '9', new List<bool> { true, false, false, true } },
                { 'A', new List<bool> { true, false, true, false } },
                { 'B', new List<bool> { true, false, true, true } },
                { 'C', new List<bool> { true, true, false, false } },
                { 'D', new List<bool> { true, true, false, true } },
                { 'E', new List<bool> { true, true, true, false } },
                { 'F', new List<bool> { true, true, true, true } }
            };

            return Packet.ParseMany(s.SelectMany(c => m[c]).ToQueue()).Single();
        }
    }

    public class Packet
    {
        public static class PacketType
        {
            public const long Sum = 0;
            public const long Product = 1;
            public const long Minimum = 2;
            public const long Maximum = 3;
            public const long Literal = 4;
            public const long GreaterThan = 5;
            public const long LessThan = 6;
            public const long Equal = 7;
        }

        public long Version { get; set; }
        public long VersionSum => Version + Subpackets.Sum(it => it.VersionSum);
        public long TypeId { get; set; }

        public long Value { get; set; }
        public readonly List<Packet> Subpackets = new();

        public static List<Packet> ParseMany(Queue<bool> q)
        {
            var result = new List<Packet>();
            while (q.Count > 7)
            {
                result.Add(new Packet(q));
            }

            return result;
        }

        public Packet(Queue<bool> input)
        {
            Version = input.DequeueList(3).ToLong();
            TypeId = input.DequeueList(3).ToLong();
            if (TypeId == PacketType.Literal) ParseLiteral(input);
            else ParseOperator(input);
        }

        private void ParseLiteral(Queue<bool> input)
        {
            var keepGoing = true;
            List<bool> value = new();
            while (keepGoing)
            {
                keepGoing = input.Dequeue();
                value.AddRange(input.DequeueList(4));
            }

            Value = value.ToLong();
        }

        private void ParseOperator(Queue<bool> input)
        {
            var mode = input.Dequeue();
            if (!mode)
            {
                var bits = input.DequeueList(15).ToLong();
                var subpackets = input.DequeueList(bits).ToQueue();
                Subpackets.AddRange(ParseMany(subpackets));
            }
            else
            {
                var npackets = input.DequeueList(11).ToLong();
                for (var i = 0; i < npackets; i++)
                {
                    Subpackets.Add(new Packet(input));
                }
            }

            Value = TypeId switch
            {
                PacketType.Equal => Subpackets[0].Value == Subpackets[1].Value ? 1 : 0,
                PacketType.GreaterThan => Subpackets[0].Value > Subpackets[1].Value ? 1 : 0,
                PacketType.LessThan => Subpackets[0].Value < Subpackets[1].Value ? 1 : 0,
                PacketType.Maximum => Subpackets.Select(it => it.Value).Max(),
                PacketType.Minimum => Subpackets.Select(it => it.Value).Min(),
                PacketType.Product => Subpackets.Select(it => it.Value).Product(),
                PacketType.Sum => Subpackets.Select(it => it.Value).Sum()
            };
        }
    }

}