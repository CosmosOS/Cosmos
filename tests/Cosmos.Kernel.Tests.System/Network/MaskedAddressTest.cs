// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.System.Network;
using NUnit.Framework;

namespace Cosmos.Kernel.Tests.System.Network;

[TestFixture]
public class MaskedAddressTest
{
    public class IPv4 : MaskedAddressTest
    {
        public class Parts : AddressTest
        {
            [TestCase(0, ExpectedResult = 0x12)]
            [TestCase(1, ExpectedResult = 0x34)]
            [TestCase(2, ExpectedResult = 0x56)]
            [TestCase(3, ExpectedResult = 0x78)]
            public byte GivenSampleIndexAndMaskedAddress_ReturnsCorrectPart(int part)
            {
                var maskedAddress = new MaskedAddress(0x12345678u);

                return maskedAddress[part];
            }

            [TestCase(-1)]
            [TestCase(4)]
            public void WhenIndexIsOutsideOfBounds_ThrowsException(int index)
            {
                var maskedAddress = new MaskedAddress(0x12345678u);

                Assert.Throws<ArgumentOutOfRangeException>(delegate
                {
                    _ = new MaskedAddress(0x12345678u)[index];
                });
            }
        }

        public class OperatorEquals : AddressTest
        {
            [TestCase(0x12345678u, 0x99999999u, ExpectedResult = false)]
            [TestCase(0x12345678u, 0x12345678u, ExpectedResult = true)]
            public bool GivenSampleValue_ReturnsExpectedResult(uint a, uint b)
            {
                return new MaskedAddress(a) == new MaskedAddress(b);
            }

            [Test]
            public void GivenSampleValue_ComparedToItself_ReturnsTrue()
            {
                var address = new MaskedAddress(0x12345678u);

                // ReSharper disable once EqualExpressionComparison
                bool actual = address == address;

                Assert.That(actual, Is.True);
            }
        }
    }
}
