// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Devices.Input;
using Cosmos.Kernel.HAL.Devices.Storage;
using Cosmos.Kernel.HAL.Devices.Usb;
using Cosmos.Kernel.HAL.Drivers.Pci;
using Cosmos.Kernel.HAL.Drivers.Usb;
using Cosmos.Kernel.HAL.Pci;
using Cosmos.Kernel.HAL.Pci.Enums;
using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.HAL.Drivers.Engine;

/// <summary>
/// The driver kit's engine. It keeps the drivers a kernel registers, runs
/// the one pass that binds them to the PCI functions and USB interfaces no
/// built-in driver took, offers the USB interfaces plugged in later the
/// same way, ends a USB binding when its device is pulled out, and
/// publishes what every device ended up owned by.
/// </summary>
/// <remarks>
/// Built-in drivers bind during HAL bring-up, before any kernel code runs,
/// and keep what they claim. Registered drivers bind late, in
/// <see cref="BindUserDrivers"/>, which Global.StartKernel runs once, on
/// the boot thread with interrupts on, before the kernel starts; a USB
/// device plugged in afterwards goes to the built-ins first, then, through
/// <see cref="KitUsbDriver"/>, to the registered USB drivers, on the USB
/// hot-plug thread, which also runs <see cref="RemoveUsbBinding"/> when one
/// is pulled out. A PCI binding is never released.
/// </remarks>
internal static class DriverCore
{
    /// <summary>
    /// What <see cref="Register(PciDriverRegistration)"/> and
    /// <see cref="Register(UsbDriverRegistration)"/> throw with when a
    /// driver's factory or Probe calls them. The pass closes registration
    /// before the first factory runs, so the closed check would refuse such
    /// a call too; the Drivers suite compares against this message to tell
    /// that the re-entrancy check is the one that did.
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

    /// <summary>
    /// The USB registrations, in registration order; null until the first
    /// one. Frozen once the pass closed registration, which is what lets the
    /// hot-plug thread read it without the lock.
    /// </summary>
    private static List<UsbDriverRegistration>? s_usbRegistrations;

    /// <summary>Set when the pass starts; from then on either Register throws.</summary>
    private static bool s_registrationClosed;

    /// <summary>
    /// Set once the pass is over. Until then <see cref="KitUsbDriver"/>
    /// leaves every interface alone: the ones present at boot are the pass's
    /// to offer, once the kernel constructor registered its drivers.
    /// </summary>
    private static volatile bool s_passCompleted;

    /// <summary>
    /// Set while a driver's factory, Probe or Remove runs. A check of the
    /// thread alone could not catch a driver registering from its own probe:
    /// the pass runs on the boot thread, which is the thread that
    /// registered. A work item needs no flag of its own: it runs only once a
    /// Probe returned Bound, inside the pass or after it, which closed
    /// registration as it began, so Register throws there already.
    /// </summary>
    private static bool s_inDriverCallback;

    /// <summary>
    /// Contexts of the functions registered drivers bound. A PCI binding is
    /// never released in this version, so they are kept for the life of the
    /// kernel, with everything reachable from them.
    /// </summary>
    private static List<PciDeviceContext>? s_boundPciContexts;

    /// <summary>The device list last published, by the pass then by the USB stack's writer; null before the pass ran.</summary>
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
    /// Takes a mouse a USB driver published back out of the mouse manager
    /// once its device left the bus: the manager's unregistration, which
    /// System installs next to <see cref="MouseSink"/> when the kernel has
    /// both mouse and USB support. Null without them, and then no USB driver
    /// can have published a mouse.
    /// </summary>
    internal static Action<PublishedMouse>? MouseWithdrawSink { get; set; }

    /// <summary>
    /// Takes a network link a USB driver published back out of the network
    /// manager, and the stack, once its device left the bus: installed like
    /// <see cref="MouseWithdrawSink"/>, next to <see cref="NetworkSink"/>.
    /// </summary>
    internal static Action<PublishedNetworkDevice>? NetworkWithdrawSink { get; set; }

    /// <summary>
    /// Every PCI function, in bus, device and function order, then every
    /// interface of every configured USB device, in the USB stack's device
    /// order, each with the driver that owns it: built-in drivers, registered
    /// drivers and the boot display reservation alike. Empty before the pass;
    /// rebuilt as USB devices come and go.
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
    /// registration, PCI or USB, or by a built-in driver.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Called from a driver's factory, Probe or Remove, or after the pass
    /// closed registration.
    /// </exception>
    internal static bool Register(PciDriverRegistration registration)
    {
        if (!CosmosFeatures.PCIEnabled)
        {
            return false;
        }

        return Register(registration.Name, registration, null);
    }

    /// <summary>
    /// Registers a USB class driver, to be offered the interfaces no
    /// built-in class driver took: those present at boot when the driver
    /// pass runs, and those of every device plugged in later. Registration
    /// is open until the pass: from the kernel's constructor, before
    /// Global.StartKernel.
    /// </summary>
    /// <param name="registration">The driver's name, factory and match table.</param>
    /// <returns>
    /// True when the driver will be offered interfaces. False when USB is
    /// compiled out, or when the name is already taken, by another
    /// registration, PCI or USB, or by a built-in driver.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Called from a driver's factory, Probe or Remove, or after the pass
    /// closed registration.
    /// </exception>
    internal static bool Register(UsbDriverRegistration registration)
    {
        // The switch alone, so ILC folds it and a kernel without USB never
        // keeps a USB registration.
        if (!CosmosFeatures.UsbEnabled)
        {
            return false;
        }

        return Register(registration.Name, null, registration);
    }

    /// <summary>
    /// The rules both kinds of registration share: one name space, since a
    /// name is what the device list identifies a driver by, and one window,
    /// from boot to the pass. Exactly one of <paramref name="pci"/> and
    /// <paramref name="usb"/> is set.
    /// </summary>
    private static bool Register(string name, PciDriverRegistration? pci, UsbDriverRegistration? usb)
    {
        // A built-in's name would make the device list lie about who owns a
        // device, and "gop" would read as the boot display's reservation.
        bool nameTaken = IsBuiltInName(name);
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
                    if (pci is not null)
                    {
                        (s_pciRegistrations ??= []).Add(pci);
                    }
                    else if (usb is not null)
                    {
                        (s_usbRegistrations ??= []).Add(usb);
                    }
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
    /// driver owns, then every USB interface no class driver took, to the
    /// registered drivers that match it, best match first, and publishes
    /// <see cref="Devices"/>. With nothing registered it touches no device
    /// and only publishes the list.
    /// </summary>
    /// <exception cref="InvalidOperationException">The pass already ran.</exception>
    internal static void BindUserDrivers()
    {
        bool alreadyRan;
        List<PciDriverRegistration>? pciRegistrations;
        List<UsbDriverRegistration>? usbRegistrations;
        using (s_lock.AcquireIrqSafe())
        {
            // Closed as the pass starts rather than as it ends: a registration
            // accepted now could not be offered the devices already walked.
            alreadyRan = s_registrationClosed;
            s_registrationClosed = true;
            pciRegistrations = s_pciRegistrations;
            usbRegistrations = s_usbRegistrations;
        }

        if (alreadyRan)
        {
            throw new InvalidOperationException("The driver pass runs once, from Global.StartKernel.");
        }

        PciDevice[] functions = FunctionsInBusOrder();
        if (pciRegistrations is null && usbRegistrations is null)
        {
            Serial.WriteString("[Drivers] No driver registered\n");
        }

        if (pciRegistrations is not null)
        {
            // Once, before the first probe, and only with a PCI driver to
            // offer anything to: whether a probe that asks for interrupts can
            // be polled when MSI-X is out of reach. A USB driver asks for none.
            InterruptPolling.CheckTimerTicks();

            for (int i = 0; i < functions.Length; i++)
            {
                OfferFunction(functions[i], pciRegistrations);
            }
        }

        // USB's switch alone, nested rather than joined to the null check:
        // ILC folds a single switch only, and this is what lets a kernel
        // without USB trim the kit's USB half.
        if (CosmosFeatures.UsbEnabled)
        {
            if (usbRegistrations is not null)
            {
                OfferUsbInterfaces(usbRegistrations);
            }
        }

        PublishDevices(functions);

        // Hot-plugged interfaces are offered from here on. The hot-plug
        // thread starts after the pass, so none can have been left behind.
        s_passCompleted = true;
    }

    /// <summary>
    /// Offers <paramref name="usbInterface"/> of a device the USB stack just
    /// enumerated after boot, which no built-in class driver took, to the
    /// registered USB drivers that match it, best match first. The
    /// <see cref="KitUsbDriver"/>'s bind, on the hot-plug thread; for a
    /// device behind a hub, inside the hub's own bind.
    /// </summary>
    /// <returns>
    /// True when a registered driver bound the interface, whose
    /// <see cref="UsbInterface.DriverContext"/> then holds the binding.
    /// Always false before the pass ran, and when no USB driver is
    /// registered.
    /// </returns>
    internal static bool OfferHotPluggedInterface(UsbDevice device, UsbInterface usbInterface)
    {
        if (!s_passCompleted)
        {
            return false;
        }

        // Read without the lock: registration closed before the pass, so the
        // list no longer changes.
        List<UsbDriverRegistration>? registrations = s_usbRegistrations;
        if (registrations is null)
        {
            return false;
        }

        return OfferUsbInterface(device, usbInterface, PathOf(device, usbInterface), registrations);
    }

    /// <summary>
    /// Rebuilds <see cref="Devices"/> from the PCI functions and the USB
    /// devices as they are now. The USB stack's device list is an unlocked
    /// list only its writer may walk, so the writer calls this: the hot-plug
    /// thread, after each device it enumerated was offered to the drivers
    /// and after each disconnect. Does nothing before the pass published the
    /// first list, which is what the boot-time enumeration runs into.
    /// </summary>
    internal static void RefreshDevices()
    {
        if (Volatile.Read(ref s_devices) is null)
        {
            return;
        }

        PublishDevices(FunctionsInBusOrder());
    }

    /// <summary>
    /// Ends the binding of a registered driver whose USB device left the
    /// bus: the <see cref="KitUsbDriver"/>'s disconnect, on the hot-plug
    /// thread, before the host controller frees the device's pipes. In this
    /// order: the context stops reporting the device present and disarms
    /// its report handlers; what the driver published leaves the mouse and
    /// network managers; its work items are dropped and its events
    /// cancelled, and a work item still running gets up to a second to
    /// finish; the driver's Remove runs; and the context is left answering
    /// Disconnected. An exception from Remove is logged with the driver's
    /// name and the interface's path, and the teardown goes on.
    /// </summary>
    /// <param name="context">The binding, as the kit kept it on the interface.</param>
    internal static void RemoveUsbBinding(UsbDeviceContext context)
    {
        context.Unplug();

        // Remove is a driver callback like Probe: Register throws from it.
        // The flag is safe to set here: the hot-plug thread is the only one
        // that runs probes once the pass is over, and it is busy with this.
        if (context.Driver is { } driver)
        {
            s_inDriverCallback = true;
            try
            {
                driver.Remove(context);
            }
            catch (Exception exception)
            {
                Serial.WriteString($"[Drivers] {context.Path} -> {context.DriverName}: Remove threw: {exception.Message}\n");
            }
            finally
            {
                s_inDriverCallback = false;
            }
        }

        context.MarkRemoved();
        Serial.WriteString($"[Drivers] {context.Path} -> {context.DriverName} removed: the device left the bus\n");
    }

    /// <summary>The path of <paramref name="function"/>: <c>pci/</c>, then segment, bus, device and function in hexadecimal.</summary>
    internal static string PathOf(PciDevice function) =>
        $"pci/0000:{function.Bus:x2}:{function.Slot:x2}.{function.Function:x}";

    /// <summary>
    /// The path of <paramref name="usbInterface"/> of <paramref name="device"/>,
    /// as Linux names it in sysfs: <c>usb/</c>, the bus, a dash, the root
    /// port then the port of each hub below it joined with dots, a colon,
    /// the configuration value, a dot and the interface number, all in
    /// decimal, such as <c>usb/1-2.1:1.0</c>. The bus is the host
    /// controller's position among the USB stack's controllers, from 1.
    /// </summary>
    internal static string PathOf(UsbDevice device, UsbInterface usbInterface) =>
        $"usb/{BusNumberOf(device.HostController)}-{PortChainOf(device)}:{device.ConfigurationValue}.{usbInterface.Number}";

    /// <summary>
    /// True when <paramref name="name"/> is a built-in driver's: a PCI
    /// owner, or a built-in USB class driver's name, which the USB stack
    /// logs and the device list shows as the owner of its interfaces.
    /// </summary>
    private static bool IsBuiltInName(string name) =>
        PciOwner.IsBuiltIn(name)
        || name == UsbHubDriver.DriverName
        || name == UsbKeyboardDriver.DriverName
        || name == UsbMassStorageDriver.DriverName;

    /// <summary>True when a registration named <paramref name="name"/> exists, PCI or USB. The caller holds <see cref="s_lock"/>.</summary>
    private static bool IsRegistered(string name)
    {
        if (s_pciRegistrations is { } pciRegistrations)
        {
            for (int i = 0; i < pciRegistrations.Count; i++)
            {
                if (pciRegistrations[i].Name == name)
                {
                    return true;
                }
            }
        }

        if (s_usbRegistrations is { } usbRegistrations)
        {
            for (int i = 0; i < usbRegistrations.Count; i++)
            {
                if (usbRegistrations[i].Name == name)
                {
                    return true;
                }
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

    /// <summary>
    /// The pass's USB step: offers every interface no class driver took, of
    /// every device the USB stack configured, device by device in its order
    /// and each device's interfaces in descriptor order. A bound interface
    /// is handed to <see cref="KitUsbDriver"/>, which lets go of it when its
    /// device leaves.
    /// </summary>
    private static void OfferUsbInterfaces(List<UsbDriverRegistration> registrations)
    {
        // A copy: a probe must not enumerate, but should one ever reach the
        // USB stack's list, the walk still sees each device once.
        IReadOnlyList<UsbDevice> present = UsbManager.Devices;
        UsbDevice[] devices = new UsbDevice[present.Count];
        for (int i = 0; i < devices.Length; i++)
        {
            devices[i] = present[i];
        }

        for (int i = 0; i < devices.Length; i++)
        {
            UsbDevice device = devices[i];
            List<UsbInterface> interfaces = device.Interfaces;
            for (int j = 0; j < interfaces.Count; j++)
            {
                UsbInterface usbInterface = interfaces[j];
                string path = PathOf(device, usbInterface);
                if (usbInterface.DriverName is { } owner)
                {
                    Serial.WriteString($"[Drivers] {path} kept by {owner}\n");
                    continue;
                }

                if (OfferUsbInterface(device, usbInterface, path, registrations))
                {
                    usbInterface.Driver = KitUsbDriver.Instance;
                }
            }
        }
    }

    /// <summary>
    /// Offers <paramref name="usbInterface"/>, which no driver owns, to each
    /// registration that matches it, best match first, until one binds. An
    /// attempt that opened an endpoint and did not bind ends the offering:
    /// the host controller cannot close an endpoint, and an interrupt IN one
    /// is already transferring, so the next candidate would find it in use.
    /// </summary>
    /// <returns>True when a driver bound the interface; its binding is then in <see cref="UsbInterface.DriverContext"/>.</returns>
    private static bool OfferUsbInterface(UsbDevice device, UsbInterface usbInterface, string path, List<UsbDriverRegistration> registrations)
    {
        List<UsbDriverRegistration> candidates = RankCandidates(device, usbInterface, registrations);
        for (int i = 0; i < candidates.Count; i++)
        {
            UsbDriverRegistration candidate = candidates[i];
            if (TryBind(device, usbInterface, path, candidate, out bool openedEndpoint))
            {
                return true;
            }

            if (openedEndpoint)
            {
                Serial.WriteString($"[Drivers] {path} -> no driver: {candidate.Name} opened an endpoint, which cannot be closed, so no other driver is offered the interface\n");
                return false;
            }
        }

        Serial.WriteString($"[Drivers] {path} -> no driver\n");
        return false;
    }

    /// <summary>
    /// The registrations that match <paramref name="usbInterface"/> of
    /// <paramref name="device"/>, most specific match first: a device match,
    /// then an interface match on class, subclass and protocol, then on
    /// class and subclass, then on class. Ties keep registration order, so
    /// the earlier registration is offered the interface first.
    /// </summary>
    private static List<UsbDriverRegistration> RankCandidates(UsbDevice device, UsbInterface usbInterface, List<UsbDriverRegistration> registrations)
    {
        List<UsbDriverRegistration> ranked = new();
        for (int i = 0; i < registrations.Count; i++)
        {
            UsbDriverRegistration registration = registrations[i];
            UsbMatchKind match = registration.BestMatch(device, usbInterface);
            if (match == UsbMatchKind.None)
            {
                continue;
            }

            // Behind every candidate that matches at least as specifically,
            // which keeps the insertion stable.
            int index = ranked.Count;
            while (index > 0 && ranked[index - 1].BestMatch(device, usbInterface) < match)
            {
                index--;
            }

            ranked.Insert(index, registration);
        }

        return ranked;
    }

    /// <summary>
    /// One binding attempt on a USB interface: builds the context, creates
    /// the driver through the registration's factory and runs its Probe. A
    /// bound interface keeps the context; any other outcome, an exception
    /// included, tears the attempt down. <paramref name="openedEndpoint"/>
    /// tells whether the attempt opened an endpoint, interrupt or bulk,
    /// whatever its outcome: a declined or failed one then ends the offering.
    /// </summary>
    /// <returns>True when the driver bound the interface.</returns>
    private static bool TryBind(UsbDevice device, UsbInterface usbInterface, string path, UsbDriverRegistration registration,
        out bool openedEndpoint)
    {
        UsbDeviceContext context = new(registration.Name, path, device, usbInterface);
        ProbeResult result;
        string? failure = null;

        s_inDriverCallback = true;
        try
        {
            UsbDriver driver = registration.Factory();
            context.Driver = driver;
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

        openedEndpoint = context.OpenedPipe;
        if (result == ProbeResult.Bound)
        {
            // Kept before the reports are armed, so the interface names its
            // owner by the time the driver's handler first runs.
            usbInterface.DriverContext = context;
            context.MarkBound();
            Serial.WriteString($"[Drivers] {path} -> {registration.Name} ({DescribeMatch(registration.BestMatch(device, usbInterface))})\n");
            return true;
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

    /// <summary>Words the kind of match a bound USB driver won with, for the log.</summary>
    private static string DescribeMatch(UsbMatchKind match) => match switch
    {
        UsbMatchKind.Device => "device match",
        UsbMatchKind.ClassWithProtocol => "interface match",
        UsbMatchKind.ClassWithSubclass => "interface subclass match",
        _ => "interface class match"
    };

    /// <summary>
    /// <paramref name="controller"/>'s bus number: its position among the
    /// USB stack's controllers, from 1, which is also the order they were
    /// brought up in.
    /// </summary>
    private static int BusNumberOf(UsbHostController controller)
    {
        IReadOnlyList<UsbHostController> controllers = UsbManager.Controllers;
        for (int i = 0; i < controllers.Count; i++)
        {
            if (controllers[i] == controller)
            {
                return i + 1;
            }
        }

        // Only a controller still being brought up is missing from the list,
        // and nothing is offered before bring-up is over.
        return 0;
    }

    /// <summary>The root port <paramref name="device"/> hangs off, then the port of each hub below it, joined with dots.</summary>
    private static string PortChainOf(UsbDevice device) =>
        device.Parent is { } hub ? $"{PortChainOf(hub)}.{device.PortNumber}" : $"{device.PortNumber}";

    /// <summary>
    /// Builds <see cref="Devices"/> from <paramref name="functions"/> and
    /// the USB stack's devices as they are now. Called by the pass, then by
    /// the USB stack's writer only.
    /// </summary>
    private static void PublishDevices(PciDevice[] functions)
    {
        List<DeviceRecord> records = new(functions.Length);
        for (int i = 0; i < functions.Length; i++)
        {
            PciDevice function = functions[i];
            records.Add(new DeviceRecord(PathOf(function), function.Owner, function.VendorId, function.DeviceId,
                function.ClassCode, function.Subclass, function.ProgIf));
        }

        // The switch alone, as in the pass.
        if (CosmosFeatures.UsbEnabled)
        {
            AddUsbRecords(records);
        }

        Volatile.Write(ref s_devices, records.ToArray());
    }

    /// <summary>
    /// Appends one record per interface of every configured USB device: its
    /// owner, the device's vendor and product ID, and the interface's class,
    /// subclass and protocol.
    /// </summary>
    private static void AddUsbRecords(List<DeviceRecord> records)
    {
        IReadOnlyList<UsbDevice> devices = UsbManager.Devices;
        for (int i = 0; i < devices.Count; i++)
        {
            UsbDevice device = devices[i];
            List<UsbInterface> interfaces = device.Interfaces;
            for (int j = 0; j < interfaces.Count; j++)
            {
                UsbInterface usbInterface = interfaces[j];
                records.Add(new DeviceRecord(PathOf(device, usbInterface), usbInterface.DriverName, device.VendorId,
                    device.ProductId, usbInterface.Class, usbInterface.Subclass, usbInterface.Protocol));
            }
        }
    }
}
