# Startup

On startup, the first thing that happens is that the BIOS of your computer loads Limine, the bootloader that Cosmos uses. From there, there is some hand-coded assembly that runs before the "Cosmos layer" kicks in. From there, the IL2CPU-ed C# entry point `Cosmos.System.Kernel.Start()` or `Sys.Kernel.Start()` is called.

> By the way, `Cosmos.System.Kernel` is an abstract class that forms the Cosmos framework. It provides a base that your OS is built on top of.

## Sys.Kernel.Start()

### What does it do?
`Kernel.Start()` does quite a bit of stuff. First, it checks if `System.String.Empty` is null. If it is null, then it will just throw an exception. If it isn't, it just continues. After that check, `Kernel.Start()` initializes the hardware bootstrap, then calls `OnBoot()`.
> The next article explains what `OnBoot()` is.

Then, `Kernel.Start()` calls your `BeforeRun()` method, after it finishes, `Kernel.Start()` enables the hardware interrupts. Then it simply does a `while (!mStopped)` loop with your `Run()` method. After that, it calls an optional method called `AfterRun()`. By default, `AfterRun()` is just empty, so don't worry about nulls or something like that. Then it finishes. All of that is also try/catched too with the `A kernel exception has occurred` message.

### Overriding it
You can override the `Kernel.Start()` method in your Kernel to suppress the standard Cosmos boot routines and get deeper control of Cosmos.
> You override it the same way you do with other methods. An extremely simple base override in your Kernel would be: `protected override void Start() {}`

The default `Kernel.Start()` method is located in `Cosmos\source\Cosmos.System2\Kernel.cs`. You can copy it and make modifications with your Kernel override.

*Last updated on 20 May 2023.*