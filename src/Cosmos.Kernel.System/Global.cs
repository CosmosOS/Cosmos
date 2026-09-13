using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.System.Graphics;

namespace Cosmos.Kernel.System;

/// <summary>
/// Global system state and initialization for Cosmos.
/// </summary>
public static class Global
{
    /// <summary>
    /// The registered kernel instance that will be started.
    /// </summary>
    private static Kernel? s_kernel;

    /// <summary>
    /// Gets the current kernel instance, or <see langword="null"/> until
    /// <see cref="RegisterKernel"/> has been called. The generated entry point
    /// registers it before <see cref="StartKernel"/> runs, so kernel code
    /// reached from <see cref="Kernel.Run"/> always sees one.
    /// </summary>
    public static Kernel? CurrentKernel => s_kernel;

    /// <summary>
    /// Registers a kernel instance to be started by the boot infrastructure.
    /// Called automatically by the generated entry point.
    /// </summary>
    /// <param name="kernel">The kernel instance to register.</param>
    public static void RegisterKernel(Kernel kernel)
    {
        Serial.WriteString("[Global] Registering kernel\n");
        s_kernel = kernel;
    }

    /// <summary>
    /// Brings up the graphical <see cref="KernelConsole"/>, which is what makes
    /// <c>Console.WriteLine</c> draw to the screen. Every other subsystem is
    /// already up by this point: the library initializer wires the managers to
    /// the HAL before any managed code runs. Called once by
    /// <see cref="Kernel.OnBoot"/>; a kernel customizes this step by overriding
    /// OnBoot instead.
    /// </summary>
    internal static void Initialize()
    {
        Serial.WriteString("[Global] Initialize() called\n");

        // Initialize graphics console (framebuffer + font)
        if (Cosmos.Kernel.Core.CosmosFeatures.GraphicsEnabled)
        {
            Serial.WriteString("[Global] Initializing KernelConsole...\n");
            if (KernelConsole.Initialize())
            {
                Serial.WriteString("[Global] KernelConsole initialized: ");
                Serial.WriteNumber((ulong)KernelConsole.Default.Cols);
                Serial.WriteString("x");
                Serial.WriteNumber((ulong)KernelConsole.Default.Rows);
                Serial.WriteString(" chars\n");
            }
            else
            {
                Serial.WriteString("[Global] WARNING: KernelConsole initialization failed!\n");
            }
        }
        else
        {
            Serial.WriteString("[Global] Graphics disabled via feature switch.\n");
        }
    }

    /// <summary>
    /// Starts the registered kernel. Called by the CosmosEntryPoint.
    /// </summary>
    public static void StartKernel()
    {
        Serial.WriteString("[Global] StartKernel called\n");

        if (s_kernel == null)
        {
            Serial.WriteString("[Global] ERROR: No kernel registered!\n");
            Serial.WriteString("[Global] Check CosmosKernelClass property in your .csproj\n");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ERROR: No kernel registered!");
            Console.WriteLine("Set <CosmosKernelClass> in your .csproj to your kernel's full type name.");
            Console.ResetColor();

            // Halt
            while (true) { }
        }

        Serial.WriteString("[Global] Starting kernel...\n");
        s_kernel.Start();

        // If kernel.Start() returns, halt the system
        Serial.WriteString("[Global] Kernel.Start() returned, halting...\n");
        while (true) { }
    }
}
