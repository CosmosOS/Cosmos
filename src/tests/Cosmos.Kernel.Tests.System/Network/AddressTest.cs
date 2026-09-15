// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Collections.Immutable;
using Cosmos.Kernel.System.Network;
using Cosmos.Kernel.System.Network.IPv4;
using Cosmos.Kernel.System.Network.IPv6;
using NUnit.Framework;

namespace Cosmos.Kernel.Tests.System.Network;

[TestFixture]
public class AddressTest
{
    public class IsIpv4 : AddressTest
    {
        [Test]
        public void WhenAddressIsIPv4_ReturnsTrue()
        {
            Address address = Address4.Zero;

            bool actual = address.IsIpv4;

            Assert.That(actual, Is.True);
        }
    }

    public class AddressType : AddressTest
    {
        [Test]
        public void WhenAddressIsIPv4_ReturnsIPv4()
        {
            Address address = Address4.Zero;

            var actual = address.AddressFamily;

            Assert.That(actual, Is.EqualTo(Kernel.System.Network.AddressFamily.IPv4));
        }
    }

    // ReSharper disable once InconsistentNaming
    public class ToUint32_FromBytes : AddressTest
    {
        [TestCase(0x50, 0x11, 0x88, 0xA0, ExpectedResult = 0x501188A0u)]
        public uint GivenSample_ReturnsCorrectValue(byte aFirst, byte aSecond, byte aThird, byte aFourth)
        {
            return Address.ToUint32(aFirst, aSecond, aThird, aFourth);
        }
    }

    public class SegmentToSpan : AddressTest
    {
        [Test]
        public void GivenSample_ReturnsCorrectValue()
        {
            Span<byte> actual = stackalloc byte[4];

            Address.SegmentToSpan(0x501188A0u, actual);

            ImmutableArray<byte> expected = [0x50, 0x11, 0x88, 0xA0];
            // no love for ref struct yet with NUnit
            Assert.That(actual.ToImmutableArray(), Is.EqualTo(expected));
        }
    }

    public class OperatorEquals : AddressTest
    {
        [TestCase(0x12345678u, 0x99999999u, ExpectedResult = false)]
        [TestCase(0x12345678u, 0x12345678u, ExpectedResult = true)]
        public bool GivenSampleValue_ReturnsExpectedResult(uint a, uint b)
        {
            return new Address4(a) == new Address4(b);
        }

        [Test]
        public void GivenSampleValue_ComparedToItself_ReturnsTrue()
        {
            var address = new Address4(0x12345678u);

            // ReSharper disable once EqualExpressionComparison
            bool actual = address == address;

            Assert.That(actual, Is.True);
        }
    }

    public class OperatorBitwiseAnd : AddressTest
    {
        [Test]
        public void GivenSampleAddressAndMask_ReturnsCorrectResult()
        {
            var address = new Address4(0x12345678u);
            var mask = new Address4(0xFF0000FFu);

            MaskedAddress actual = address & mask;

            bool equals = actual == new MaskedAddress(0x12000078u);
            Assert.That(equals, Is.True);
        }
    }

    public class Parse : AddressTest
    {
        [Test]
        public void GivenDottedDecimal_ReturnsAddress4()
        {
            Address? actual = Address.Parse("192.168.1.10");

            Assert.That(actual, Is.EqualTo(new Address4(192, 168, 1, 10)));
        }

        [Test]
        public void GivenColonSeparatedHex_ReturnsAddress6()
        {
            Address? actual = Address.Parse("::1");

            Assert.That(actual, Is.EqualTo(Address6.Loopback));
        }

        [TestCase("")]
        [TestCase("not an address")]
        [TestCase("1.2.3")]
        public void GivenNeitherForm_ReturnsNull(string source)
        {
            Assert.That(Address.Parse(source), Is.Null);
        }
    }

    public new class ToString : AddressTest
    {
        [Test]
        public void GivenAddress4ThroughTheBaseType_WritesDottedDecimal()
        {
            Address address = new Address4(0xC0A8010Au);

            string actual = address.ToString();

            Assert.That(actual, Is.EqualTo("192.168.1.10"));
        }
    }

    public class CompareTo : AddressTest
    {
        [Test]
        public void GivenTwoAddress6_OrdersByValue()
        {
            Address low = Address6.Parse("fe80::1")!;
            Address high = Address6.Parse("fe80::2")!;

            Assert.Multiple(() =>
            {
                Assert.That(low.CompareTo(high), Is.LessThan(0));
                Assert.That(high.CompareTo(low), Is.GreaterThan(0));
                Assert.That(low.CompareTo(Address6.Parse("fe80::1")), Is.Zero);
            });
        }

        [Test]
        public void GivenTwoAddress4_OrdersByValue()
        {
            Address low = new Address4(10, 0, 2, 2);
            Address high = new Address4(10, 0, 2, 15);

            Assert.That(low.CompareTo(high), Is.LessThan(0));
        }

        [Test]
        public void GivenDifferentFamilies_Throws()
        {
            Address v4 = new Address4(10, 0, 2, 2);
            Address v6 = Address6.Parse("fe80::1")!;

            Assert.Throws<ArgumentException>(() => v4.CompareTo(v6));
        }
    }

    public class EqualityOperator : AddressTest
    {
        [Test]
        public void GivenTwoEqualAddress6_IsTrue()
        {
            Address a = Address6.Parse("fe80::5054:ff:fe12:3456")!;
            Address b = new Address6(0xFE80_0000, 0, 0x5054_00FF, 0xFE12_3456);

            Assert.Multiple(() =>
            {
                Assert.That(a == b, Is.True);
                Assert.That(a != b, Is.False);
            });
        }

        [Test]
        public void GivenTwoDifferentAddress6_IsFalse()
        {
            Address a = Address6.Parse("fe80::1")!;
            Address b = Address6.Parse("fe80::2")!;

            Assert.That(a == b, Is.False);
        }

        [Test]
        public void GivenDifferentFamilies_IsFalse()
        {
            Assert.That(Address4.Zero == Address6.Zero, Is.False);
        }

        [Test]
        public void GivenNull_IsFalse()
        {
            // Callers write `address != null` on the non-nullable type, so the
            // operator has to answer for a null operand.
            Address a = Address6.Parse("fe80::1")!;

            Assert.Multiple(() =>
            {
                Assert.That(a == null!, Is.False);
                Assert.That(a != null!, Is.True);
            });
        }
    }
}
