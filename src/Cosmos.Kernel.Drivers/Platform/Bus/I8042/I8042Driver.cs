// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Platform;
using Cosmos.Kernel.HAL.DriverKit.Ps2;
using Cosmos.Kernel.System;

namespace Cosmos.Kernel.Drivers.Platform.Bus.I8042;

/// <summary>
/// The 8042 keyboard controller over the driver kit: binds the x64 machine
/// description's <c>platform:i8042@60</c> node, brings the controller up
/// (ports disabled, the output buffer flushed, translation on, the self
/// test, the interface tests), connects its two lines, and publishes one
/// <c>ps2:</c> node per port whose test passed, with a <see cref="Ps2Access"/>
/// the keyboard and mouse drivers talk through. The controller is the only
/// code that touches ports 0x60 and 0x64 (<c>X64PowerOps.Reboot</c> writes
/// 0xFE to 0x64 behind it, which no driver can claim against). Everything it
/// holds for one controller lives on an <see cref="I8042State"/> in
/// <see cref="DeviceBinding.DriverState"/>. <see cref="Probe"/> and
/// <see cref="OnDetach"/> run in thread context on the kit worker.
/// </summary>
[Driver]
public sealed class I8042Driver : Driver
{
    // --- Constants ---

    /// <summary>The compatible string of the 8042 node the machine description publishes.</summary>
    public const string CompatibleString = "pnp0303";

    // --- Private fields ---

    private readonly DeviceMatch[] _matches =
    [
        PlatformMatch.Compatible(CompatibleString),
    ];

    // --- Properties ---

    /// <inheritdoc/>
    public override string Name => nameof(I8042Driver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    // --- Public methods ---

    /// <summary>
    /// Brings the controller up and publishes a Ps2 node per port whose
    /// interface test passed. Thread context on the kit worker; a declined
    /// or failed result makes the kit release everything acquired here.
    /// </summary>
    /// <param name="binding">The 8042 node and the kit facilities for it.</param>
    /// <returns>Bound with the children published; declined when both input features are off; failed when the controller did not come up or no port passed its test.</returns>
    public override ProbeResult Probe(DeviceBinding binding)
    {
        // 1. The switches, one per if: a compound guard does not fold in
        //    Debug IL.
        if (!KernelFeatures.Keyboard)
        {
            if (!KernelFeatures.Mouse)
            {
                return ProbeResult.Declined("keyboard and mouse support are off");
            }
        }

        // 2. The two one-port windows.
        RegisterWindow data = binding.MapRegisters(0);
        RegisterWindow control = binding.MapRegisters(1);

        // 3. The state and its lock.
        I8042State state = new(binding, data, control, binding.CreateLock());
        binding.DriverState = state;

        // 4. Both ports disabled while the controller is set up.
        if (!state.WriteCommand(I8042State.DisableKeyboardPort))
        {
            return ProbeResult.Failed("the controller does not accept commands");
        }

        if (!state.WriteCommand(I8042State.DisableAuxiliaryPort))
        {
            return ProbeResult.Failed("the controller does not accept commands");
        }

        // 5. Whatever a device left in the output buffer.
        state.Flush();

        // 6. The configuration byte: both interrupt enables clear for now,
        //    translation on so the keyboard port delivers set 1 scan codes
        //    whatever firmware left.
        if (!state.WriteCommand(I8042State.ReadConfiguration))
        {
            return ProbeResult.Failed("the controller did not answer the configuration read");
        }

        if (!state.TryReadReply(out byte configuration))
        {
            return ProbeResult.Failed("the controller did not answer the configuration read");
        }

        bool secondPortPossible = (configuration & I8042State.AuxiliaryClockDisabled) != 0;
        configuration = (byte)((configuration & ~(I8042State.KeyboardInterruptEnable | I8042State.AuxiliaryInterruptEnable)) | I8042State.Translation);
        if (!state.WriteCommand(I8042State.WriteConfiguration, configuration))
        {
            return ProbeResult.Failed("the controller does not accept commands");
        }

        // 7. The self test, then the configuration again: some controllers
        //    reset it on 0xAA. The rewrite's outcome is not checked here:
        //    step 14 writes the configuration once more.
        if (!state.WriteCommand(I8042State.SelfTest))
        {
            return ProbeResult.Failed("self test failed");
        }

        bool ok = state.TryReadReply(out byte reply);
        if (!ok || reply != I8042State.SelfTestPassed)
        {
            return ProbeResult.Failed("self test failed");
        }

        state.WriteCommand(I8042State.WriteConfiguration, configuration);

        // 8. Dual channel: the clock bit clears when the second port is
        //    enabled.
        state.IsDualChannel = false;
        if (secondPortPossible)
        {
            state.WriteCommand(I8042State.EnableAuxiliaryPort);
            state.WriteCommand(I8042State.ReadConfiguration);
            ok = state.TryReadReply(out reply);
            state.IsDualChannel = ok && (reply & I8042State.AuxiliaryClockDisabled) == 0;
            if (state.IsDualChannel)
            {
                state.WriteCommand(I8042State.DisableAuxiliaryPort);
            }
        }

        // 9. The interface tests.
        state.WriteCommand(I8042State.TestKeyboardPort);
        ok = state.TryReadReply(out reply);
        bool keyboardPortOk = ok && reply == I8042State.PortTestPassed;
        if (!keyboardPortOk)
        {
            binding.Log($"port kbd test failed: 0x{reply:x2}");
        }

        bool auxiliaryPortOk = false;
        if (state.IsDualChannel)
        {
            state.WriteCommand(I8042State.TestAuxiliaryPort);
            ok = state.TryReadReply(out reply);
            auxiliaryPortOk = ok && reply == I8042State.PortTestPassed;
            if (!auxiliaryPortOk)
            {
                binding.Log($"port aux test failed: 0x{reply:x2}");
            }
        }

        if (!keyboardPortOk)
        {
            if (!auxiliaryPortOk)
            {
                return ProbeResult.Failed("no port passed its interface test");
            }
        }

        // 10.
        state.Flush();

        // 11. The accesses.
        state.KeyboardAccess = keyboardPortOk ? new Ps2Access(Ps2Port.Keyboard, state) : null;
        state.AuxiliaryAccess = auxiliaryPortOk ? new Ps2Access(Ps2Port.Auxiliary, state) : null;

        // 12. The lines: one handler for both, the status register picking
        //     the port. Interrupt driven only with both needed lines
        //     connected, so no port depends on a line the other port's
        //     bytes may land on.
        IReadOnlyList<InterruptSource> interrupts = binding.Node.Interrupts;
        bool keyboardLine = keyboardPortOk && interrupts.Count > 0 && binding.TryRequestInterrupt(interrupts[0], state.OnInterrupt, out _);
        bool auxiliaryLine = auxiliaryPortOk && interrupts.Count > 1 && binding.TryRequestInterrupt(interrupts[1], state.OnInterrupt, out _);
        state.SetInterruptDriven((!keyboardPortOk || keyboardLine) && (!auxiliaryPortOk || auxiliaryLine));

        // 13. The periodic drain otherwise; a handle connected for one line
        //     stays connected and harmless.
        if (!state.InterruptDriven)
        {
            WorkItem drain = binding.CreateWorkItem(state.DrainOnWorker);
            state.DrainWork = drain;
            state.SetPolledPeriodically(binding.TrySchedulePeriodic(I8042State.PollPeriodMilliseconds, drain));
        }

        // 14. The configuration with the interrupt enables of the connected
        //     lines, then the ports that passed. Translation stays set.
        if (state.InterruptDriven)
        {
            if (keyboardPortOk)
            {
                configuration |= I8042State.KeyboardInterruptEnable;
            }

            if (auxiliaryPortOk)
            {
                configuration |= I8042State.AuxiliaryInterruptEnable;
            }
        }

        state.WriteCommand(I8042State.WriteConfiguration, configuration);
        if (keyboardPortOk)
        {
            state.WriteCommand(I8042State.EnableKeyboardPort);
        }

        if (auxiliaryPortOk)
        {
            state.WriteCommand(I8042State.EnableAuxiliaryPort);
        }

        // 15. A byte that landed before the enables has no edge on an
        //     edge-triggered line: it reaches its access now.
        state.Drain();

        // 16. The children, keyboard first; from the worker the offers queue
        //     behind this probe.
        if (state.KeyboardAccess is Ps2Access keyboardAccess)
        {
            state.KeyboardNode = binding.PublishChild(new Ps2Identity(Ps2Port.Keyboard), [], keyboardAccess.InterruptsForPublish(), keyboardAccess);
        }

        if (state.AuxiliaryAccess is Ps2Access auxiliaryAccess)
        {
            state.AuxiliaryNode = binding.PublishChild(new Ps2Identity(Ps2Port.Auxiliary), [], auxiliaryAccess.InterruptsForPublish(), auxiliaryAccess);
        }

        // 17.
        string channels = state.IsDualChannel ? "dual channel" : "single channel";
        string wake;
        if (state.InterruptDriven)
        {
            if (keyboardLine)
            {
                wake = auxiliaryLine ? "lines 1 and 12" : "line 1";
            }
            else
            {
                wake = "line 12";
            }
        }
        else if (state.PolledPeriodically)
        {
            wake = $"polled every {I8042State.PollPeriodMilliseconds} ms";
        }
        else
        {
            wake = "no interrupt and no timer";
        }

        binding.Log($"{channels}, translation on, {wake}");
        return ProbeResult.Bound;
    }

    /// <summary>
    /// Quiesces the controller when the hardware is still there: both ports
    /// disabled and the interrupt enables cleared. Thread context on the kit
    /// worker; the kit tore the two children down and disconnected the two
    /// line handles before this runs, and the windows are still valid.
    /// </summary>
    /// <param name="binding">The binding being torn down.</param>
    /// <param name="reason">Why, and whether the hardware is still there.</param>
    public override void OnDetach(DeviceBinding binding, DetachReason reason)
    {
        if (binding.DriverState is I8042State state && reason.HardwarePresent)
        {
            state.QuiesceOnDetach();
        }
    }
}
