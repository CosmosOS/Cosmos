// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Core.ARM64.Cpu;

/// <summary>
/// GIC architecture register-programming values shared by the GICv2 and
/// GICv3 drivers: register layout strides, bitmap masks, and the kernel's
/// default interrupt priority policy.
/// </summary>
internal static class GicArch
{
    /// <summary>Byte stride between consecutive 32-bit GIC registers.</summary>
    internal const uint RegisterStrideBytes = 4;

    /// <summary>All-ones mask covering every interrupt bit in a 32-bit enable/pending register.</summary>
    internal const uint AllInterruptsMask = 0xFFFFFFFF;

    /// <summary>GICD_ICFGR per-interrupt 2-bit field value selecting edge-triggered (0b10), common to GICv2 GICD_ICFGR and GICv3 GICD_ICFGR/GICR_ICFGRn.</summary>
    internal const uint IcfgrEdgeTriggered = 2u;

    /// <summary>Priority mask value allowing all interrupt priorities (0xFF = lowest priority threshold), written to GICC_PMR (v2) / ICC_PMR_EL1 (v3).</summary>
    internal const uint PriorityMaskAllowAll = 0xFF;

    /// <summary>Default interrupt priority the kernel programs (lower value = higher priority); single source of the 0xA0 policy shared by IPRIORITYR words and LPI config-table entries.</summary>
    internal const byte DefaultPriority = 0xA0;

    /// <summary>Default priority replicated into each byte of a GICD_IPRIORITYR / GICR_IPRIORITYR word.</summary>
    internal const uint DefaultPriorityAllBytes = 0x01010101u * DefaultPriority;
}
