// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.System.Storage;
using NUnit.Framework;

namespace Cosmos.Kernel.Tests.System.Storage;

[TestFixture]
public class DiskRankTest
{
    // PCI addresses as the storage manager packs them: q35's on-board AHCI
    // at 00:1f.2, an added controller at 00:03.0, an NVMe at 00:04.0.
    private static readonly uint s_onBoardAhci = DiskRank.PackPciAddress(0, 0x1f, 2);
    private static readonly uint s_addedAhci = DiskRank.PackPciAddress(0, 3, 0);
    private static readonly uint s_nvme = DiskRank.PackPciAddress(0, 4, 0);

    /// <summary>
    /// Registers the ranks in the given order the way the storage manager
    /// does, each at its insertion index, and returns their sequences in
    /// the resulting order.
    /// </summary>
    private static ulong[] Order(params DiskRank[] registrations)
    {
        List<DiskRank> ordered = [];
        foreach (DiskRank rank in registrations)
        {
            ordered.Insert(DiskRank.InsertionIndex(ordered.ToArray(), rank), rank);
        }

        return ordered.Select(rank => rank.Sequence).ToArray();
    }

    private static DiskRank Ahci(uint address, ulong sequence) => new(false, DiskKind.Ahci, address, sequence);

    private static DiskRank Nvme(uint address, ulong sequence) => new(false, DiskKind.Nvme, address, sequence);

    private static DiskRank Usb(ulong sequence) => new(true, DiskKind.Other, DiskRank.NoPciAddress, sequence);

    private static DiskRank Unknown(ulong sequence) => new(false, DiskKind.Other, DiskRank.NoPciAddress, sequence);

    public class CompareTo : DiskRankTest
    {
        [Test]
        public void WhenOnlyOneIsRemovable_TheFixedDiskRanksFirst()
        {
            Assert.That(Unknown(9).CompareTo(Usb(0)), Is.Negative);
            Assert.That(Usb(0).CompareTo(Unknown(9)), Is.Positive);
        }

        [Test]
        public void WhenBothAreFixed_AhciRanksBeforeNvmeBeforeOther()
        {
            Assert.That(Ahci(s_onBoardAhci, 9).CompareTo(Nvme(s_nvme, 0)), Is.Negative);
            Assert.That(Nvme(s_nvme, 9).CompareTo(Unknown(0)), Is.Negative);
        }

        [Test]
        public void WhenKindsMatch_TheLowerPciAddressRanksFirst()
        {
            Assert.That(Ahci(s_addedAhci, 9).CompareTo(Ahci(s_onBoardAhci, 0)), Is.Negative);
        }

        [Test]
        public void WhenAllElseMatches_TheEarlierRegistrationRanksFirst()
        {
            Assert.That(Ahci(s_addedAhci, 0).CompareTo(Ahci(s_addedAhci, 1)), Is.Negative);
            Assert.That(Ahci(s_addedAhci, 1).CompareTo(Ahci(s_addedAhci, 1)), Is.Zero);
        }
    }

    public class InsertionIndex : DiskRankTest
    {
        // The boot order since AHCI moved to the driver kit: the NVMe
        // namespace and the USB stick register from System's initializer,
        // the SATA disk from the driver pass after them.
        [Test]
        public void WhenAhciRegistersAfterNvmeAndUsb_AhciIsFirst()
        {
            ulong[] order = Order(Nvme(s_nvme, 0), Usb(1), Ahci(s_addedAhci, 2));

            Assert.That(order, Is.EqualTo(new ulong[] { 2, 0, 1 }));
        }

        // The order HAL registered disks in before the move, which the rule
        // must keep: AHCI, NVMe, then USB.
        [Test]
        public void WhenRegisteredInTheOldBootOrder_TheOrderIsUnchanged()
        {
            ulong[] order = Order(Ahci(s_addedAhci, 0), Nvme(s_nvme, 1), Usb(2));

            Assert.That(order, Is.EqualTo(new ulong[] { 0, 1, 2 }));
        }

        [Test]
        public void WhenOneControllerPublishesSeveralDisks_TheyKeepPortOrder()
        {
            ulong[] order = Order(Ahci(s_addedAhci, 0), Ahci(s_addedAhci, 1), Ahci(s_addedAhci, 2));

            Assert.That(order, Is.EqualTo(new ulong[] { 0, 1, 2 }));
        }

        [Test]
        public void WhenTwoControllersRegisterOutOfBusOrder_BusOrderWins()
        {
            ulong[] order = Order(Ahci(s_onBoardAhci, 0), Ahci(s_addedAhci, 1));

            Assert.That(order, Is.EqualTo(new ulong[] { 1, 0 }));
        }

        // A disk registered through the public API, whose controller the
        // manager cannot see, is a fixed disk of no known kind.
        [Test]
        public void WhenAnUnknownDiskRegistersAfterUsb_ItRanksBeforeUsbAndAfterNvme()
        {
            ulong[] order = Order(Nvme(s_nvme, 0), Usb(1), Unknown(2));

            Assert.That(order, Is.EqualTo(new ulong[] { 0, 2, 1 }));
        }

        [Test]
        public void WhenUsbSticksComeAndGo_TheyStayInRegistrationOrderBehindFixedDisks()
        {
            ulong[] order = Order(Usb(0), Ahci(s_addedAhci, 1), Usb(2));

            Assert.That(order, Is.EqualTo(new ulong[] { 1, 0, 2 }));
        }
    }
}
