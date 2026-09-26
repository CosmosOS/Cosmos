// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Drivers.Pci;
using Cosmos.Kernel.HAL.Pci;
using Cosmos.Kernel.HAL.Pci.Enums;
using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.HAL.Drivers.Engine;

/// <summary>
/// The driver kit's engine. It keeps the drivers a kernel registers, runs
/// the one pass that binds them to the PCI functions no built-in driver
/// took, and publishes what every function ended up owned by.
/// </summary>
/// <remarks>
/// Built-in drivers bind during HAL bring-up, before any kernel code runs,
/// and keep what they claim. Registered drivers bind late, in
/// <see cref="BindUserDrivers"/>, which Global.StartKernel runs once, on
/// the boot thread with interrupts on, before the kernel starts.
/// </remarks>
internal static class DriverCore
{
    /// <summary>
    /// What <see cref="Register"/> throws with when a driver's factory or
    /// Probe calls it. The pass closes registration before the first factory
    /// runs, so the closed check would refuse such a call too; the Drivers
    /// suite compares against this message to tell that the re-entrancy
    /// check is the one that did.
    /// </summary>
    internal const string RegisterFromDriverCallbackMessage = "A driver cannot register drivers from its factory or its Probe.";

    /// <summary>
    /// Makes a registration's checks and its append one step against the
    /// pass closing registration, should a thread the kernel started
    /// register while the boot thread begins the pass. IRQ-safe so the
    /// holder is not preempted while another thread spins on it.
    /// </summary>
    private static SchedSpinLock s_lock;

    /// <summary>The PCI registrations, in registration order; null until the first one.</summary>
    private static List<PciDriverRegistration>? s_pciRegistrations;

    /// <summary>Set when the pass starts; from then on <see cref="Register"/> throws.</summary>
    private static bool s_registrationClosed;

    /// <summary>
    /// Set while a driver's factory or Probe runs. A check of the thread
    /// alone could not catch a driver registering from its own probe: the
    /// pass runs on the boot thread, which is the thread that registered.
    /// A work item needs no flag of its own: it runs only once a Probe
    /// returned Bound, inside the pass, which closed registration as it
    /// began, so Register throws there already.
    /// </summary>
    private static bool s_inDriverCallback;

    /// <summary>
    /// Contexts of the functions registered drivers bound. A PCI binding is
    /// never released in this version, so they are kept for the life of the
    /// kernel, with everything reachable from them.
    /// </summary>
    private static List<PciDeviceContext>? s_boundPciContexts;

    /// <summary>The device list the pass published; null before it ran.</summary>
    private static DeviceRecord[]? s_devices;

    /// <summary>
    /// Where the mice drivers publish are delivered: the mouse manager's
    /// registration, which System installs from its library initializer when
    /// the kernel has mouse support, since HAL cannot reference System. Null
    /// without it, and <see cref="DeviceContext.PublishMouse"/> then throws.
    /// </summary>
    internal static Action<PublishedMouse>? MouseSink { get; set; }

    /// <summary>
    /// Where the network links drivers publish are delivered: the network
    /// manager's registration, installed like <see cref="MouseSink"/> when the
    /// kernel has network support. Null without it, and
    /// <see cref="DeviceContext.PublishNetworkLink"/> then throws.
    /// </summary>
    internal static Action<PublishedNetworkDevice>? NetworkSink { get; set; }

    /// <summary>
    /// Every PCI function, in bus, device and function order, with the
    /// driver that owns it once the pass ran: built-in drivers, registered
    /// drivers and the boot display reservation alike. Empty before the pass.
    /// </summary>
    internal static IReadOnlyList<DeviceRecord> Devices => Volatile.Read(ref s_devices) ?? [];

    /// <summary>
    /// Registers a PCI driver, to be offered the functions no built-in driver
    /// owns when the driver pass runs. Registration is open until then: from
    /// the kernel's constructor, before Global.StartKernel.
    /// </summary>
    /// <param name="registration">The driver's name, factory and match table.</param>
    /// <returns>
    /// True when the driver will take part in the pass. False when PCI is
    /// compiled out, or when the name is already taken, by another
    /// registration or by a built-in driver.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Called from a driver's factory or Probe, or after the pass closed
    /// registration.
    /// </exception>
    internal static bool Register(PciDriverRegistration registration)
    {
        if (!CosmosFeatures.PCIEnabled)
        {
            return false;
        }

        string name = registration.Name;

        // A built-in's name would make the device list lie about who owns a
        // function, and "gop" would read as the boot display's reservation.
        bool nameTaken = PciOwner.IsBuiltIn(name);
        bool inDriverCallback;
        bool closed;
        using (s_lock.AcquireIrqSafe())
        {
            inDriverCallback = s_inDriverCallback;
            closed = s_registrationClosed;
            if (!inDriverCallback && !closed && !nameTaken)
            {
                nameTaken = IsRegistered(name);
                if (!nameTaken)
                {
                    (s_pciRegistrations ??= []).Add(registration);
                }
            }
        }

        if (inDriverCallback)
        {
            throw new InvalidOperationException(RegisterFromDriverCallbackMessage);
        }

        if (closed)
        {
            throw new InvalidOperationException("Driver registration closed when the driver pass ran; register drivers from the kernel's constructor.");
        }

        if (nameTaken)
        {
            Serial.WriteString($"[Drivers] Refused to register {name}: the name is taken\n");
            return false;
        }

        Serial.WriteString($"[Drivers] Registered {name}\n");
        return true;
    }

    /// <summary>
    /// The driver pass. Closes registration, offers every PCI function no
    /// driver owns to the registered drivers that match it, best match
    /// first, and publishes <see cref="Devices"/>. With nothing registered it
    /// touches no device and only publishes the list.
    /// </summary>
    /// <exception cref="InvalidOperationException">The pass already ran.</exception>
    internal static void BindUserDrivers()
    {
        bool alreadyRan;
        List<PciDriverRegistration>? registrations;
        using (s_lock.AcquireIrqSafe())
        {
            // Closed as the pass starts rather than as it ends: a registration
            // accepted now could not be offered the functions already walked.
            alreadyRan = s_registrationClosed;
            s_registrationClosed = true;
            registrations = s_pciRegistrations;
        }

        if (alreadyRan)
        {
            throw new InvalidOperationException("The driver pass runs once, from Global.StartKernel.");
        }

        PciDevice[] functions = FunctionsInBusOrder();
        if (registrations is null)
        {
            Serial.WriteString("[Drivers] No driver registered\n");
        }
        else
        {
            // Once, before the first probe, and only with a driver to offer
            // anything to: whether a probe that asks for interrupts can be
            // polled when MSI-X is out of reach.
            InterruptPolling.CheckTimerTicks();

            for (int i = 0; i < functions.Length; i++)
            {
                OfferFunction(functions[i], registrations);
            }
        }

        PublishDevices(functions);
    }

    /// <summary>The path of <paramref name="function"/>: <c>pci/</c>, then segment, bus, device and function in hexadecimal.</summary>
    internal static string PathOf(PciDevice function) =>
        $"pci/0000:{function.Bus:x2}:{function.Slot:x2}.{function.Function:x}";

    /// <summary>True when a registration named <paramref name="name"/> exists. The caller holds <see cref="s_lock"/>.</summary>
    private static bool IsRegistered(string name)
    {
        List<PciDriverRegistration>? registrations = s_pciRegistrations;
        if (registrations is null)
        {
            return false;
        }

        for (int i = 0; i < registrations.Count; i++)
        {
            if (registrations[i].Name == name)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// The enumerated functions sorted by bus, device and function.
    /// Enumeration walks a bridge's secondary bus as soon as it finds the
    /// bridge, so its own order is depth-first, not numeric. Empty when PCI
    /// was never set up, as with the Interrupts switch off.
    /// </summary>
    private static PciDevice[] FunctionsInBusOrder()
    {
        PciDevice[]? devices = PciManager.Devices;
        if (devices is null)
        {
            return [];
        }

        PciDevice[] sorted = new PciDevice[PciManager.Count];
        for (int i = 0; i < sorted.Length; i++)
        {
            sorted[i] = devices[i];
        }

        // Insertion sort: at most 64 functions, already nearly in order.
        for (int i = 1; i < sorted.Length; i++)
        {
            PciDevice current = sorted[i];
            ulong key = BusOrderKey(current);
            int j = i - 1;
            while (j >= 0 && BusOrderKey(sorted[j]) > key)
            {
                sorted[j + 1] = sorted[j];
                j--;
            }

            sorted[j + 1] = current;
        }

        return sorted;
    }

    private static ulong BusOrderKey(PciDevice function) =>
        ((ulong)function.Bus << 16) | ((ulong)function.Slot << 8) | function.Function;

    /// <summary>
    /// Offers <paramref name="function"/> to each registration that matches
    /// it, best match first, until one binds. A function a driver already
    /// owns, including the boot display's reservation, is left alone.
    /// </summary>
    private static void OfferFunction(PciDevice function, List<PciDriverRegistration> registrations)
    {
        string path = PathOf(function);
        string? owner = function.Owner;
        if (owner is not null)
        {
            Serial.WriteString($"[Drivers] {path} kept by {owner}\n");
            return;
        }

        // Building a context turns decoding off to size the BARs, and a
        // bridge with decoding off stops forwarding to every function behind
        // it, bound ones included. The seam binds endpoints only.
        if (function.HeaderType != PciHeaderType.Normal)
        {
            Serial.WriteString($"[Drivers] {path} is a bridge, not offered\n");
            return;
        }

        List<PciDriverRegistration> candidates = RankCandidates(function, registrations);
        for (int i = 0; i < candidates.Count; i++)
        {
            if (TryBind(function, path, candidates[i]))
            {
                return;
            }
        }

        Serial.WriteString($"[Drivers] {path} -> no driver\n");
    }

    /// <summary>
    /// The registrations that match <paramref name="function"/>, most
    /// specific match first: a device match, then a class match with a
    /// programming interface, then a class match. Ties keep registration
    /// order, so the earlier registration is offered the function first.
    /// </summary>
    private static List<PciDriverRegistration> RankCandidates(PciDevice function, List<PciDriverRegistration> registrations)
    {
        List<PciDriverRegistration> ranked = new();
        for (int i = 0; i < registrations.Count; i++)
        {
            PciDriverRegistration registration = registrations[i];
            PciMatchKind match = registration.BestMatch(function);
            if (match == PciMatchKind.None)
            {
                continue;
            }

            // Behind every candidate that matches at least as specifically,
            // which keeps the insertion stable.
            int index = ranked.Count;
            while (index > 0 && ranked[index - 1].BestMatch(function) < match)
            {
                index--;
            }

            ranked.Insert(index, registration);
        }

        return ranked;
    }

    /// <summary>
    /// One binding attempt: builds the context, creates the driver through
    /// the registration's factory and runs its Probe. A bound function is
    /// claimed under the registration's name and its context kept; any
    /// other outcome, an exception included, tears the attempt down.
    /// </summary>
    /// <returns>True when the driver bound the function.</returns>
    private static bool TryBind(PciDevice function, string path, PciDriverRegistration registration)
    {
        PciDeviceContext context = PciDeviceContext.Create(registration.Name, path, function);
        ProbeResult result;
        string? failure = null;

        s_inDriverCallback = true;
        try
        {
            PciDriver driver = registration.Factory();
            context.BeginProbe();
            result = driver.Probe(context);
        }
        catch (Exception exception)
        {
            result = ProbeResult.Failed;
            failure = exception.Message;
        }
        finally
        {
            s_inDriverCallback = false;
        }

        if (result == ProbeResult.Bound)
        {
            if (function.TryClaim(registration.Name))
            {
                context.MarkBound();
                (s_boundPciContexts ??= []).Add(context);
                Serial.WriteString($"[Drivers] {path} -> {registration.Name} ({DescribeMatch(registration.BestMatch(function))})\n");
                return true;
            }

            // Only a built-in that claims lazily, from another thread, can
            // get here; TryClaim logged who it was.
            result = ProbeResult.Failed;
            failure = "another driver took the function during the probe";
        }

        context.TearDown();
        if (result == ProbeResult.Declined)
        {
            Serial.WriteString($"[Drivers] {path} -> {registration.Name} declined\n");
        }
        else
        {
            Serial.WriteString($"[Drivers] {path} -> {registration.Name} failed: {failure ?? "Probe returned Failed"}\n");
        }

        return false;
    }

    /// <summary>Words the kind of match a bound driver won with, for the log.</summary>
    private static string DescribeMatch(PciMatchKind match) => match switch
    {
        PciMatchKind.Device => "device match",
        PciMatchKind.ClassWithInterface => "class and interface match",
        _ => "class match"
    };

    /// <summary>Builds <see cref="Devices"/> from <paramref name="functions"/> as the pass left them.</summary>
    private static void PublishDevices(PciDevice[] functions)
    {
        DeviceRecord[] records = new DeviceRecord[functions.Length];
        for (int i = 0; i < functions.Length; i++)
        {
            PciDevice function = functions[i];
            records[i] = new DeviceRecord(PathOf(function), function.Owner, function.VendorId, function.DeviceId,
                function.ClassCode, function.Subclass, function.ProgIf);
        }

        Volatile.Write(ref s_devices, records);
    }
}
