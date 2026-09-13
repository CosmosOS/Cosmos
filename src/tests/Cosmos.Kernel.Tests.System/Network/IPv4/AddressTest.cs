// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.System.Network.IPv4;
using NUnit.Framework;

namespace Cosmos.Kernel.Tests.System.Network.IPv4;

public class AddressTest
{
    [TestFixture]
    public class Constructors : AddressTest
    {
        [Test]
        public void GivenPackedValue_SplitsItMostSignificantOctetFirst()
        {
            Address actual = new(0xC0A8010Au);

            Assert.That(actual.Parts, Is.EqualTo(new byte[] { 192, 168, 1, 10 }));
        }

        [Test]
        public void GivenBufferAndOffset_ReadsFourBytesFromTheOffset()
        {
            byte[] buffer = [0xFF, 10, 0, 2, 15, 0xFF];

            Address actual = new(buffer, 1);

            Assert.That(actual, Is.EqualTo(new Address(10, 0, 2, 15)));
        }

        [TestCase(3)]
        [TestCase(5)]
        public void GivenSpanOfWrongLength_Throws(int length)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = new Address(new byte[length]));
        }
    }

    [TestFixture]
    public class Id : AddressTest
    {
        [TestCase(192, 168, 1, 10, ExpectedResult = 0xC0A8010Au)]
        [TestCase(0, 0, 0, 0, ExpectedResult = 0u)]
        [TestCase(255, 255, 255, 255, ExpectedResult = 0xFFFFFFFFu)]
        public uint GivenOctets_PacksThemMostSignificantFirst(byte first, byte second, byte third, byte fourth)
        {
            return new Address(first, second, third, fourth).Id;
        }
    }

    [TestFixture]
    public class Equality : AddressTest
    {
        [Test]
        public void GivenTheSameOctets_TwoInstancesAreEqualAndHashAlike()
        {
            Address left = new(10, 0, 2, 15);
            Address right = new(0x0A00020Fu);

            Assert.That(left, Is.EqualTo(right));
            Assert.That(left.GetHashCode(), Is.EqualTo(right.GetHashCode()));
        }

        [Test]
        public void GivenDifferentOctets_TwoInstancesAreNotEqual()
        {
            Assert.That(new Address(10, 0, 2, 15), Is.Not.EqualTo(new Address(10, 0, 2, 16)));
        }

        [Test]
        public void GivenNull_IsNotEqual()
        {
            Assert.That(new Address(10, 0, 2, 15).Equals(null), Is.False);
        }
    }

    [TestFixture]
    public class CompareTo : AddressTest
    {
        [TestCase(0x0A00020Fu, 0x0A00020Fu, ExpectedResult = 0)]
        [TestCase(0x0A000210u, 0x0A00020Fu, ExpectedResult = 1)]
        [TestCase(0x0A00020Fu, 0x0A000210u, ExpectedResult = -1)]
        public int GivenTwoAddresses_OrdersByPackedValue(uint left, uint right)
        {
            return new Address(left).CompareTo(new Address(right));
        }

        [Test]
        public void GivenNull_OrdersAfterIt()
        {
            Assert.That(new Address(0u).CompareTo(null), Is.EqualTo(1));
        }
    }

    [TestFixture]
    public class Formatting : AddressTest
    {
        [TestCase(0xC0A8010Au, ExpectedResult = "192.168.1.10")]
        [TestCase(0u, ExpectedResult = "0.0.0.0")]
        public string GivenAddress_WritesDottedDecimal(uint packed)
        {
            return new Address(packed).ToString();
        }
    }

    [TestFixture]
    public class IsBroadcastAddress : AddressTest
    {
        [Test]
        public void GivenAllOnes_IsTrue()
        {
            Assert.That(Address.Broadcast.IsBroadcastAddress(), Is.True);
        }

        [Test]
        public void GivenAnyOtherAddress_IsFalse()
        {
            Assert.That(new Address(255, 255, 255, 254).IsBroadcastAddress(), Is.False);
        }
    }
}
