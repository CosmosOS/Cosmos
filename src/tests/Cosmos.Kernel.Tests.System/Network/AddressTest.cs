// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Collections.Immutable;
using Cosmos.Kernel.System.Network;
using Cosmos.Kernel.System.Network.IPv4;
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
}
