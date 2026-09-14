// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Collections.Immutable;
using Cosmos.Kernel.System.Network;
using Cosmos.Kernel.System.Network.IPv4;
using NUnit.Framework;

namespace Cosmos.Kernel.Tests.System.Network.IPv4;

[TestFixture]
public class Address4Test
{
    public class Constructors : Address4Test
    {
        [Test]
        public void GivenPackedValue_SplitsItMostSignificantOctetFirst()
        {
            Address4 actual = new(0xC0A8010Au);

            MaskedAddress parts = actual.Parts;
            byte[] octets = [parts[0], parts[1], parts[2], parts[3]];
            Assert.That(octets, Is.EqualTo(new byte[] { 192, 168, 1, 10 }));
        }

        [Test]
        public void GivenBufferAndOffset_ReadsFourBytesFromTheOffset()
        {
            byte[] buffer = [0xFF, 10, 0, 2, 15, 0xFF];

            Address4 actual = new(buffer, 1);

            Assert.That(actual, Is.EqualTo(new Address4(10, 0, 2, 15)));
        }

        [TestCase(3)]
        [TestCase(5)]
        public void GivenSpanOfWrongLength_Throws(int length)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = new Address4(new byte[length]));
        }
    }

    public class Segment1 : Address4Test
    {
        [TestCase(192, 168, 1, 10, ExpectedResult = 0xC0A8010Au)]
        [TestCase(0, 0, 0, 0, ExpectedResult = 0u)]
        [TestCase(255, 255, 255, 255, ExpectedResult = 0xFFFFFFFFu)]
        public uint GivenOctets_PacksThemMostSignificantFirst(byte first, byte second, byte third, byte fourth)
        {
            return new Address4(first, second, third, fourth).Segment1;
        }
    }

    public class Parse : Address4Test
    {
        [Test]
        public void GivenCorrectSampleInDecimal_ReturnsCorrectAddress4()
        {
            const string source = "12.34.56.78";

            var actual = Address4.Parse(source, AddressNumericStyle.Dec);

            Assert.That(actual, Is.EqualTo(new Address4(12, 34, 56, 78)));
        }
        [Test]
        public void GivenCorrectSampleInHex_ReturnsCorrectAddress4()
        {
            const string source = "12.34.56.A0";

            var actual = Address4.Parse(source, AddressNumericStyle.Hex);

            Assert.That(actual, Is.EqualTo(new Address4(0x12, 0x34, 0x56, 0xA0)));
        }

        [TestCase("12.34.56")]
        [TestCase("12.34.56.78.99")]
        public void GivenInvalidDecSamples_ReturnsNull(string source)
        {
            var actual = Address4.Parse(source, AddressNumericStyle.Dec);

            Assert.That(actual, Is.Null);
        }
    }

    public class ToBytes : Address4Test
    {
        [Test]
        public void GivenSampleAddress_ToBytesIsCorrect()
        {
            var source = new Address4(0x12, 0x34, 0x56, 0xA0);

            var actual = source.ToBytes().ToImmutableArray();

            Assert.That(actual, Is.EqualTo(new byte[] { 0x12, 0x34, 0x56, 0xA0 }));
        }
    }

    public class Equality : Address4Test
    {
        [Test]
        public void GivenTheSameOctets_TwoInstancesAreEqualAndHashAlike()
        {
            Address4 left = new(10, 0, 2, 15);
            Address4 right = new(0x0A00020Fu);

            Assert.That(left, Is.EqualTo(right));
            Assert.That(left.GetHashCode(), Is.EqualTo(right.GetHashCode()));
        }

        [Test]
        public void GivenDifferentOctets_TwoInstancesAreNotEqual()
        {
            Assert.That(new Address4(10, 0, 2, 15), Is.Not.EqualTo(new Address4(10, 0, 2, 16)));
        }

        [Test]
        public void GivenNull_IsNotEqual()
        {
            Assert.That(new Address4(10, 0, 2, 15).Equals(null), Is.False);
        }
    }

    public class ToStringDefault : Address4Test
    {
        [TestCase(0x123456A0u, ExpectedResult = "18.52.86.160")]
        [TestCase(0xC0A8010Au, ExpectedResult = "192.168.1.10")]
        [TestCase(0u, ExpectedResult = "0.0.0.0")]
        public string GivenSampleAddress_ReturnsStringRepresentation(uint ip)
        {
            return new Address4(ip).ToString();
        }
    }
    public new class ToString : Address4Test
    {
        [TestCase(0x123456A0u, AddressNumericStyle.Dec, false, ExpectedResult = "18.52.86.160")]
        [TestCase(0x123456A0u, AddressNumericStyle.Dec, true, ExpectedResult = "018.052.086.160")]
        [TestCase(0x023406A0u, AddressNumericStyle.Hex, false, ExpectedResult = "2.34.6.a0")]
        [TestCase(0x023406A0u, AddressNumericStyle.Hex, true, ExpectedResult = "02.34.06.a0")]
        public string GivenSampleAddress_ReturnsStringRepresentation(uint ip, AddressNumericStyle style, bool leadingZeros)
        {
            return new Address4(ip).ToString(style, leadingZeros);
        }
    }

    public class CompareTo : Address4Test
    {
        [TestCase(0x123456A0u, 0x123456A0u, ExpectedResult = 0)]
        [TestCase(0x123456A1u, 0x123456A0u, ExpectedResult = 1)]
        [TestCase(0x123456A2u, 0x123456A0u, ExpectedResult = 1)]
        [TestCase(0x123456A0u, 0x123456A1u, ExpectedResult = -1)]
        [TestCase(0x123456A0u, 0x123456A2u, ExpectedResult = -1)]
        public int GivenSampleData_ReturnsCompareResult(uint a, uint b)
        {
            var addressA = new Address4(a);
            var addressB = new Address4(b);

            return addressA.CompareTo(addressB);
        }

        [Test]
        public void GivenNull_OrdersAfterIt()
        {
            Assert.That(new Address4(0u).CompareTo(null), Is.EqualTo(1));
        }
    }

    public class IsBroadcastAddress : Address4Test
    {
        [Test]
        public void GivenAllOnes_IsTrue()
        {
            Assert.That(Address4.Broadcast.IsBroadcastAddress, Is.True);
        }

        [Test]
        public void GivenAnyOtherAddress_IsFalse()
        {
            Assert.That(new Address4(255, 255, 255, 254).IsBroadcastAddress, Is.False);
        }
    }
}
