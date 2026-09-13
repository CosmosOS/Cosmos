using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.Core.Memory.GarbageCollector;
using Cosmos.Kernel.Core.Runtime;
using Cosmos.Kernel.HAL;
using Cosmos.Kernel.System.Keyboard;
using Cosmos.Kernel.System.Mouse;
using Cosmos.Kernel.System.Network;
using Cosmos.Kernel.System.Storage;
using Cosmos.Kernel.System.Timer;

namespace Internal.Runtime.CompilerHelpers;

/// <summary>
/// This class is responsible for initializing the library and its dependencies. It is called by the runtime before any managed code is executed.
/// </summary>
internal class LibraryInitializer
{
    /// <summary>
    /// Initialize all enabled services provided by Cosmos.Kernel.System, such as TimerManager, KeyboardManager, and NetworkManager. This method is called by the runtime before any managed code is executed.
    /// </summary>
    public static void InitializeLibrary()
    {
        IPlatformInitializer? initializer = PlatformHAL.Initializer;
        if (initializer is not null)
        {
            // Initialize Timer Manager (skipped if CosmosEnableTimer=false)
            if (CosmosFeatures.TimerEnabled)
            {
                Serial.WriteString("[KERNEL]   - Initializing timer manager...\n");
                TimerManager.RegisterTimer(initializer.CreateTimer());
            }

            using (InternalCpu.DisableInterruptsScope())
            {

                // Initialize Keyboard Manager and register platform keyboards
                if (KeyboardManager.IsEnabled)
                {
                    Serial.WriteString("[KERNEL]   - Initializing keyboard manager...\n");
                    KeyboardManager.Initialize();
                    IKeyboardDevice[] keyboards = initializer.GetKeyboardDevices();
                    foreach (IKeyboardDevice keyboard in keyboards)
                    {
                        KeyboardManager.RegisterKeyboard(keyboard);
                    }
                }

                // Initialize Mouse Manager and register mouse
                if (MouseManager.IsEnabled)
                {
                    Serial.WriteString("[KERNEL]   - Initializing mouse manager...\n");
                    MouseManager.Initialize();
                    IMouseDevice[] mice = initializer.GetMouseDevices();
                    foreach (IMouseDevice mouse in mice)
                    {
                        MouseManager.RegisterMouse(mouse);
                    }
                }

                // Initialize Network Manager and register platform network device
                if (NetworkManager.IsEnabled)
                {
                    Serial.WriteString("[KERNEL]   - Initializing network manager...\n");
                    NetworkManager.Initialize();
                    INetworkDevice? networkDevice = initializer.GetNetworkDevice();
                    if (networkDevice is not null)
                    {
                        NetworkManager.RegisterDevice(networkDevice);
                    }
                }

                // Initialize Storage Manager (manager-level state only)
                if (StorageManager.IsEnabled)
                {
                    Serial.WriteString("[KERNEL]   - Initializing storage manager...\n");
                    StorageManager.Initialize();
                }
            }

            // Storage device registration runs OUTSIDE the
            // DisableInterruptsScope: ScanPartitions issues real I/O
            // (LBA 0 read for MBR/GPT detection), and interrupt-driven
            // drivers like NVMe need IF=1 / DAIF.I=0 to receive
            // completion IRQs. Disposing the scope only RESTORES the
            // prior state: on ARM64 IRQs were still masked from boot
            // at this point, so explicitly unmask before doing I/O.
            // The kernel re-enables IRQs again in Kernel.Start; this
            // call is idempotent.
            if (StorageManager.IsEnabled)
            {
                if (InterruptManager.IsEnabled)
                {
                    InternalCpu.EnableInterrupts();
                }
                StorageManager.RegisterHalDevices();
            }
        }
    }
}
