// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Boot;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Input;
using Cosmos.Kernel.System.Network;
using Cosmos.Kernel.System.Storage;
using Cosmos.Kernel.System.Timers;

namespace Internal.Runtime.CompilerHelpers;

/// <summary>
/// Boot-time initializer of Cosmos.Kernel.System. ILC finds this type by name and calls <see cref="InitializeLibrary"/> before the kernel's entry point runs.
/// </summary>
internal static class LibraryInitializer
{
    /// <summary>
    /// Initialize all enabled services provided by Cosmos.Kernel.System, such as TimerManager, KeyboardManager, and NetworkManager.
    /// </summary>
    public static void InitializeLibrary()
    {
        IPlatformInitializer? initializer = PlatformHAL.Initializer;
        if (initializer is not null)
        {
            // Skipped if CosmosEnableTimer=false.
            if (CosmosFeatures.TimerEnabled)
            {
                Serial.WriteString("[KERNEL]   - Initializing timer manager...\n");
                TimerManager.RegisterTimer(initializer.CreateTimer());
            }

            using (InternalCpu.DisableInterruptsScope())
            {
                // The keyboard manager's consumer registers every keyboard a
                // kit driver publishes (the PS/2, virtio and USB keyboards)
                // once the driver stage runs.
                if (KeyboardManager.IsEnabled)
                {
                    Serial.WriteString("[KERNEL]   - Initializing keyboard manager...\n");
                    KeyboardManager.Initialize();
                }

                // The mouse manager's consumer registers every pointer a kit
                // driver publishes (the PS/2 and virtio mice).
                if (MouseManager.IsEnabled)
                {
                    Serial.WriteString("[KERNEL]   - Initializing mouse manager...\n");
                    MouseManager.Initialize();
                }

                // The network manager's consumer registers every interface a
                // kit driver publishes once the driver stage runs.
                if (NetworkManager.IsEnabled)
                {
                    Serial.WriteString("[KERNEL]   - Initializing network manager...\n");
                    NetworkManager.Initialize();
                }

                // The display manager's consumer lists the firmware framebuffer
                // the engine publishes at its start and every display a kit
                // driver publishes once the driver stage runs.
                if (DisplayManager.IsEnabled)
                {
                    Serial.WriteString("[KERNEL]   - Initializing display manager...\n");
                    DisplayManager.Initialize();
                }

                // The storage manager's consumer registers every block device
                // a kit driver publishes once the driver stage runs.
                if (StorageManager.IsEnabled)
                {
                    Serial.WriteString("[KERNEL]   - Initializing storage manager...\n");
                    StorageManager.Initialize();
                }
            }
        }
    }
}
