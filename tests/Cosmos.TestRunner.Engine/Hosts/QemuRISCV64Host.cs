namespace Cosmos.TestRunner.Engine.Hosts;

/// <summary>
/// QEMU host for the RISC-V 64 virt machine. The run is the arm64 one, the
/// ISO behind virtio-scsi and the UART log watched for the suite markers;
/// <see cref="Cosmos.Tools.Launcher.QemuLauncher"/> builds the machine,
/// firmware and devices from the architecture name.
/// </summary>
public class QemuRISCV64Host : QemuARM64Host
{
    /// <inheritdoc/>
    public override string Architecture => "riscv64";
}
