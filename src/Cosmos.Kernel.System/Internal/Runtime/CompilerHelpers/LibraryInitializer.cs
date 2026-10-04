using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.Core.Memory.GarbageCollector;
using Cosmos.Kernel.Core.Runtime;
using Cosmos.Kernel.HAL;
using Cosmos.Kernel.HAL.Interfaces;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Graphics;
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

                // Initialize Network Manager; its consumer registers every
                // interface a kit driver publishes once the driver stage runs.
                if (NetworkManager.IsEnabled)
                {
                    Serial.WriteString("[KERNEL]   - Initializing network manager...\n");
                    NetworkManager.Initialize();
                }

                // Initialize Display Manager; its consumer lists the firmware
                // framebuffer the engine publishes at its start and every
                // display a kit driver publishes once the driver stage runs.
                if (DisplayManager.IsEnabled)
                {
                    Serial.WriteString("[KERNEL]   - Initializing display manager...\n");
                    DisplayManager.Initialize();
                }

                // Initialize Storage Manager (manager-level state only)
                if (StorageManager.IsEnabled)
                {
                    Serial.WriteString("[KERNEL]   - Initializing storage manager...\n");
                    StorageManager.Initialize();
                }
            }
        }
    }
}
