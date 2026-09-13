// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Cpu.Enums;

// TODO: Protect IRQs like memory and ports are
// TODO: Make IRQs so they are not hookable, and instead release high priority threads like FreeBSD (When we get threading)
/// <summary>
/// EFlags Enum.
/// </summary>
internal enum EFlagsEnum : uint
{
    /// <summary>
    /// Set by arithmetic instructions, can be carry or borrow.
    /// </summary>
    Carry = 1,

    /// <summary>
    ///  Set by most CPU instructions if the LSB of the destination operand contain an even number of 1's.
    /// </summary>
    Parity = 1 << 2,

    /// <summary>
    /// Set when an arithmetic carry or borrow has been generated out of the four LSBs.
    /// </summary>
    AuxilliaryCarry = 1 << 4,

    /// <summary>
    /// Set to 1 if an arithmetic result is zero, and reset otherwise.
    /// </summary>
    Zero = 1 << 6,

    /// <summary>
    /// Set to 1 if the last arithmetic result was positive, and reset otherwise.
    /// </summary>
    Sign = 1 << 7,

    /// <summary>
    /// When set to 1, permits single step operations.
    /// </summary>
    Trap = 1 << 8,

    /// <summary>
    /// When set to 1, maskable hardware interrupts will be handled, and ignored otherwise.
    /// </summary>
    InterruptEnable = 1 << 9,

    /// <summary>
    /// When set to 1, strings is processed from highest address to lowest, and from lowest to highest otherwise.
    /// </summary>
    Direction = 1 << 10,

    /// <summary>
    /// Set to 1 if arithmetic overflow has occurred in the last operation.
    /// </summary>
    Overflow = 1 << 11,

    /// <summary>
    /// Set to 1 when one system task invoke another by CALL instruction.
    /// </summary>
    NestedTag = 1 << 14,

    /// <summary>
    /// When set to 1, enables the option turn off certain exceptions while debugging.
    /// </summary>
    Resume = 1 << 16,

    /// <summary>
    /// When set to 1, Virtual8086Mode is enabled.
    /// </summary>
    Virtual8086Mode = 1 << 17,

    /// <summary>
    /// When set to 1, enables alignment check.
    /// </summary>
    AlignmentCheck = 1 << 18,

    /// <summary>
    /// When set, the program will receive hardware interrupts.
    /// </summary>
    VirtualInterrupt = 1 << 19,

    /// <summary>
    /// When set, indicate that there is deferred interrupt pending.
    /// </summary>
    VirtualInterruptPending = 1 << 20,

    /// <summary>
    /// When set, indicate that CPUID instruction is available.
    /// </summary>
    ID = 1 << 21
}
