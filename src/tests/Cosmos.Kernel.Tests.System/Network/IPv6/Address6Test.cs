// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Collections.Immutable;
using Cosmos.Kernel.System.Network;
using Cosmos.Kernel.System.Network.IPv4;
using Cosmos.Kernel.System.Network.IPv6;
using Cosmos.Kernel.Tests.System.Network.IPv4;
using NUnit.Framework;

namespace Cosmos.Kernel.Tests.System.Network.IPv6;

[TestFixture]
public class Address6Test
{
    public class CountSeparators : Address6Test
    {
        [TestCase("::", ExpectedResult = 2)]
        [TestCase("", ExpectedResult = 0)]
        [TestCase(":1123:", ExpectedResult = 2)]
        [TestCase("1123:", ExpectedResult = 1)]
        public int GivenSampleSource_CountsSeparatorsCorrectly(string source)
        {
            return Address6.CountSeparators(source);
        }
    }

    public class SplitByZeroGroupsAbbreviation : Address6Test
    {
        [TestCaseSource(nameof(TestValidCases))]
        public void GivenSampleSource_SplitsRangesAccordingly(TestValidCaseData dt)
        {
            Address6.SplitByZeroGroupsAbbreviation(dt.Addr, dt.Ranges.AsSpan(),
                out var actualLeft, out var actualRight);

            Assert.That(actualLeft.ToImmutableArray(), Is.EquivalentTo(dt.ExpectedLeft));
            Assert.That(actualRight.ToImmutableArray(), Is.EquivalentTo(dt.ExpectedRight));
        }

        [TestCaseSource(nameof(TestValidCases))]
        public void GivenSampleSource_ReturnsSuccess(TestValidCaseData dt)
        {
            bool? actual = Address6.SplitByZeroGroupsAbbreviation(dt.Addr, dt.Ranges.AsSpan(),
                out var _, out var _);

            Assert.That(actual, Is.True);
        }

        public record struct TestValidCaseData(
            string Addr,
            ImmutableArray<Range> Ranges,
            ImmutableArray<Range> ExpectedLeft,
            ImmutableArray<Range> ExpectedRight);

        private static IEnumerable<TestValidCaseData> TestValidCases()
        {
            yield return CreateTestCaseData("::", [], []);
            yield return CreateTestCaseData("0010::", [new Range(0, 4)], []);
            yield return CreateTestCaseData("0010::0020", [new(0, 4)], [new(6, 10)]);
            yield return CreateTestCaseData("0010:0020::0030", [new(0, 4), new(5, 9)], [new(11, 15)]);
        }

        private static TestValidCaseData CreateTestCaseData(string source, ImmutableArray<Range> expectedLeft,
            ImmutableArray<Range> expectedRight)
        {
            var fragments = source.AsSpan().Split(':');
            return new(source, [.. fragments], expectedLeft, expectedRight);
        }

        [TestCase(":::")]
        [TestCase("0001:::")]
        [TestCase(":::0002")]
        [TestCase("0001:::0002")]
        [TestCase("0001:::0002")]
        [TestCase("0001")]
        [TestCase("0001:2")]
        [TestCase("0:1:2:3:4:5:6")]
        [TestCase(":0")]
        [TestCase("0:1:2:3:4:5:")]
        public void GivenInvalidAddresses_ReturnsFalse(string address)
        {
            var fragments = address.AsSpan().Split(':');
            ReadOnlySpan<Range> ranges = [.. fragments];

            bool? actual = Address6.SplitByZeroGroupsAbbreviation(address, ranges,
                out var _, out var _);

            Assert.That(actual, Is.False);
        }
    }

    public class Parse : Address6Test
    {
        [TestCase(":::")]
        [TestCase("0001:::")]
        [TestCase(":::0002")]
        [TestCase("0001:::0002")]
        [TestCase("0001:::0002")]
        [TestCase("0001")]
        [TestCase("0001:2")]
        [TestCase("0:1:2:3:4:5:6")]
        [TestCase(":0")]
        [TestCase("0:1:2:3:4:5:")]
        [TestCase(":::00022")]
        [TestCase("::ffff:c0000280")]
        public void GivenInvalidAddresses_ReturnsNull(string address)
        {
            var actual = Address6.Parse(address);

            Assert.That(actual, Is.Null);
        }

        [TestCaseSource(nameof(TestCases))]
        public void GivenSampleSource_ReturnsCorrectAddress(TestValidCaseData dt)
        {
            var actual = Address6.Parse(dt.Addr);

            Assert.That(actual, Is.EqualTo(new Address6(dt.ExpectedSegments.AsSpan())));
        }

        public record struct TestValidCaseData(
            string Addr,
            ImmutableArray<uint> ExpectedSegments);

        private static IEnumerable<TestValidCaseData> TestCases()
        {
            yield return new("::", [0, 0, 0, 0]);
            yield return new("0010::", [0x0010_0000, 0, 0, 0]);
            yield return new("0010::0020", [0x0010_0000, 0, 0, 0x0020]);
            yield return new("0010:0020::0030", [0x0010_0020, 0, 0, 0x0000_0030]);
            yield return new("::ffff:c000:0280", [0x0, 0x0, 0x0000FFFF, 0xC0000280]); // 192.0.2.128
        }
    }

    public class AddressType : Address6Test
    {
        [TestCase("::ffff:c000:0280", ExpectedResult = IPv6AddressType.EmbeddedIPv4)]
        [TestCase("2000::", ExpectedResult = IPv6AddressType.GlobalUnicast)]
        [TestCase("2100::", ExpectedResult = IPv6AddressType.GlobalUnicast)]
        [TestCase("2100::000a", ExpectedResult = IPv6AddressType.GlobalUnicast)]
        [TestCase("FE80::", ExpectedResult = IPv6AddressType.LinkLocal)]
        [TestCase("FE80::000a", ExpectedResult = IPv6AddressType.LinkLocal)]
        [TestCase("0:0:0:0:0:0:0:1", ExpectedResult = IPv6AddressType.Loopback)]
        [TestCase("0:0:0:0:0:0:0:0", ExpectedResult = IPv6AddressType.Unspecified)]
        [TestCase("FF00::", ExpectedResult = IPv6AddressType.WellKnown)]
        [TestCase("FF02::", ExpectedResult = IPv6AddressType.WellKnown)]
        [TestCase("ff02:0:0:0:0:1:ff00:0", ExpectedResult = IPv6AddressType.SolicitedNode)]
        public IPv6AddressType? GivenAddress_ReturnsCorrectAddressType(string address)
        {
            var target = Address6.Parse(address);

            return target?.AddressType;
        }
    }

    public class ToStringDefault : Address6Test
    {
        [TestCase("ff02:0:0:0:0:1:ff00:0", ExpectedResult = "ff02::1:ff00:0")]
        public string? GivenSampleAddress_ReturnsStringRepresentation(string address)
        {
            var target = Address6.Parse(address);

            return target?.ToString();
        }
    }

    public new class ToString : Address4Test
    {
        [TestCaseSource(nameof(TestCases))]
        public void GivenSampleAddress_ReturnsStringRepresentation(TestCaseData data)
        {
            var address = new Address6(data.Addr.AsSpan());

            string actual = address.ToString(data.LeadingZeros, data.GroupZeros);

            Assert.That(actual, Is.EqualTo(data.ExpectedResult));
        }

        public readonly record struct TestCaseData(
            ImmutableArray<ushort> Addr,
            bool LeadingZeros,
            bool GroupZeros,
            string? ExpectedResult);

        private static IEnumerable<TestCaseData> TestCases()
        {
            yield return new TestCaseData([0, 0, 0, 0, 0, 0, 0, 0], false, false,
                "0:0:0:0:0:0:0:0");
            yield return new TestCaseData([0, 0, 0, 0, 0, 0, 0, 0], true, false,
                "0000:0000:0000:0000:0000:0000:0000:0000");
            yield return new TestCaseData([0, 0, 0, 0, 0, 0, 0, 0], false, true,
                "::");
            yield return new TestCaseData([0, 0, 0, 0, 0, 0, 0, 0], true, false,
                "0000:0000:0000:0000:0000:0000:0000:0000");
            yield return new TestCaseData([0, 0, 0, 0, 0, 0, 0, 0], false, true,
                "::");
            yield return new TestCaseData([0, 0, 0, 0, 0x8010, 0, 0, 0], false, false,
                "0:0:0:0:8010:0:0:0");
            yield return new TestCaseData([0, 0, 0, 0, 0x8010, 0, 0, 0], false, true,
                "::8010:0:0:0");
            yield return new TestCaseData([0, 0xff, 0, 0, 0x8010, 0, 0, 0], false, true,
                "0:ff:0:0:8010::");
        }
    }

    public class FindLargestZeroGroup : Address6Test
    {
        [TestCaseSource(nameof(TestCases))]
        public void GivenSampleAddress_FindsLargestGroup(TestCaseData data)
        {
            var actual = Address6.FindLargestZeroGroup(data.Addr.AsSpan());

            Assert.That(actual, Is.EqualTo(data.ExpectedResult));
        }

        public readonly record struct TestCaseData(
            ImmutableArray<ushort> Addr,
            (int Start, int Length)? ExpectedResult);

        private static IEnumerable<TestCaseData> TestCases()
        {
            yield return new([0, 0, 0, 0, 0, 0, 0, 0], (0, 8));
            yield return new([0, 0, 0, 8, 0, 0, 0, 0], (4, 4));
            yield return new([0, 0, 0, 8, 0, 2, 0, 0], (0, 3));
            yield return new([0, 1, 0, 8, 0, 2, 0, 0], (6, 2));
            yield return new([0x0, 0x0, 0x0, 0xFFFF, 0xC000, 0x0280], (0, 3)); // 192.0.2.128
        }
    }
}
