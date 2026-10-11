// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.ComponentModel;
using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.System.Internal;

/// <summary>
/// Boot plumbing, not for kernel code: registers and starts the kernel for
/// the entry point the build generates into every kernel's assembly
/// (<c>CosmosEntryPoint.Main</c>). It is public only because that generated
/// code compiles into the kernel's own assembly. Kernel code reaches the
/// running kernel through <see cref="Kernel.Current"/> instead.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class KernelEntry
{
    /// <summary>
    /// Registers <paramref name="kernel"/> as <see cref="Kernel.Current"/>,
    /// then runs its <see cref="Kernel.Start"/>, and never returns: if
    /// <see cref="Kernel.Start"/> comes back, this spins, since returning from
    /// the entry point would reach the runtime's shutdown path, which
    /// allocates. Called once, by the generated entry point.
    /// </summary>
    /// <param name="kernel">The kernel instance the generated entry point constructed.</param>
    public static void Start(Kernel kernel)
    {
        Serial.WriteString("[KernelEntry] Registering kernel\n");
        Kernel.Current = kernel;

        Serial.WriteString("[KernelEntry] Start called\n");

        if (kernel is null)
        {
            Serial.WriteString("[KernelEntry] ERROR: No kernel registered!\n");
            Serial.WriteString("[KernelEntry] Check CosmosKernelClass property in your .csproj\n");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ERROR: No kernel registered!");
            Console.WriteLine("Set <CosmosKernelClass> in your .csproj to your kernel's full type name.");
            Console.ResetColor();

            while (true)
            {
            }
        }

        Serial.WriteString("[KernelEntry] Starting kernel...\n");
        kernel.Start();

        Serial.WriteString("[KernelEntry] Kernel.Start() returned, halting...\n");
        while (true)
        {
        }
    }
}
