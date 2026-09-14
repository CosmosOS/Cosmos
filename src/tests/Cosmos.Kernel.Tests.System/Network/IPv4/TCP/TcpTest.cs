// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.System.Network.IPv4;
using Cosmos.Kernel.System.Network.IPv4.TCP;
using NUnit.Framework;

namespace Cosmos.Kernel.Tests.System.Network.IPv4.TCP;

public class TcpTest
{
    private Tcp _target = null!;

    [SetUp]
    public void Setup()
    {
        _target = Tcp.CreateConnection(0, 0, new Address4(1, 2, 3, 4), new Address4(1, 2, 3, 4));
    }

    [TestFixture]
    public class AppendToData : TcpTest
    {
        [Test]
        public void WhenBothData_AndOtherAreEmpty_DataIsEmpty()
        {
            _target.AppendToData([]);

            Assert.That(_target.Data.Length, Is.EqualTo(0));
        }

        [Test]
        public void WhenDataIsNotEmpty_AndOtherIsEmpty_DataDoesNotChange()
        {
            _target.AppendToData([0, 1]);

            _target.AppendToData([]);

            Assert.That(_target.Data.ToArray(), Is.EquivalentTo([0, 1]));
        }

        [Test]
        public void WhenDataIsNotEmpty_AndOtherIsNotEmpty_OtherIsAppendedToData()
        {
            _target.AppendToData([0, 1]);

            _target.AppendToData([2, 3]);

            Assert.That(_target.Data.ToArray(), Is.EquivalentTo([0, 1, 2, 3]));
        }
    }

    [TestFixture]
    public class AdvanceDataOffset : TcpTest
    {
        [Test]
        public void WhenAdvancingByZero_NoChangesAreMade()
        {
            _target.AppendToData([0, 1]);

            _target.AdvanceDataOffset(0);

            Assert.That(_target.Data.ToArray(), Is.EquivalentTo([0, 1]));
        }

        [Test]
        public void WhenAdvancingByOneAndLengthIsTwo_OnlyLastElementRemains()
        {
            _target.AppendToData([0, 1]);

            _target.AdvanceDataOffset(1);

            Assert.That(_target.Data.ToArray(), Is.EquivalentTo([1]));
        }
        [Test]
        public void WhenAdvancingByTwoAndLengthIsTwo_DataLengthIsZero()
        {
            _target.AppendToData([0, 1]);

            _target.AdvanceDataOffset(2);

            Assert.That(_target.Data.Length, Is.Zero);
        }
    }
}
