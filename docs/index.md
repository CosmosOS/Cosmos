---
_layout: landing
---

Welcome to the Cosmos gen3 wiki! 

Check out the [Roadmap](roadmap.md) to see our progress toward the first release 🚀.

The documentation is split in two parts, depending on what you want to do with gen3:

- **[User Guide](articles/user/install.md)**: you want to **build your own OS** with Cosmos: installing the toolchain, using the filesystem, debugging your kernel.
- **[Contributor Docs](articles/dev/install-dev.md)**: you want to **contribute to Cosmos itself** or understand its internals: architecture, build pipeline, runtime subsystems.

## User Guide

Everything you need to create, build and run your own Cosmos kernel:

 - [Installation Guide](articles/user/install.md): set up the toolchain and create your first kernel from VS Code.
 - [Kernel Startup](articles/user/startup.md): the boot chain and the `BeforeRun`/`Run`/`AfterRun` lifecycle.
 - [File System](articles/user/filesystem.md): mount a disk and use the standard .NET `System.IO` API (`File`, `Directory`, streams).
 - [Network](articles/user/network.md): DHCP, UDP and TCP through the standard .NET `System.Net.Sockets` API, plus DNS.
 - [Graphics](articles/user/graphics.md): draw shapes, text and images on the screen with the Canvas API.
 - [Keyboard](articles/user/keyboard.md): read lines, key events and layouts through `Console` and `KeyboardManager`.
 - [Mouse](articles/user/mouse.md): pointer position, buttons and scroll wheel through `MouseManager`.
 - [Debugging with VSCode and QEMU](articles/user/debugging.md): set breakpoints in your kernel with remote GDB.

## Contributor Docs

Architecture and internals, for contributors and the curious:

 - [Dev Container Setup](articles/dev/install-dev.md): build the framework from source.
 - [Kernel Project Layout](articles/dev/kernel-project-layout.md): the layered project graph.
 - [Coding Guidelines](articles/dev/coding-guidelines.md): style and architecture patterns.
 - [Plugs](articles/dev/plugs.md): the IL-level method replacement system.
 - [Testing](articles/dev/testing.md): unit tests and QEMU kernel test suites.
 - [Public API Tracking](articles/dev/public-api.md): declared surface files, package validation, versioned docs.
 - [Garbage Collector](articles/dev/garbage-collector.md): the mark-and-sweep GC.
 - [Garbage Collector - Precise Stack Scan](articles/dev/garbage-collector-gcinfo.md): how GCInfo makes the triggering thread's stack scan exact.
 - [Garbage Collector - Glossary](articles/dev/garbage-collector-glossary.md): background notes on the GC concepts the articles build on.
 - [Scheduler](articles/dev/scheduler.md): the preemptive, pluggable scheduler.
 - [Scheduler - Writing a Scheduler](articles/dev/scheduler-plugging.md): how to implement and install a scheduling policy.
 - [Scheduler - Glossary](articles/dev/scheduler-glossary.md): background notes on the scheduling concepts the article builds on.
 - [Kernel Compilation Steps](articles/dev/build/kernel-compilation-steps.md): C# to bootable ISO, end to end.
 - [Cosmos.Build.Asm](articles/dev/build/asm-build.md), [Cosmos.Build.GCC](articles/dev/build/gcc-build.md), [Cosmos.Build.Patcher](articles/dev/build/patcher-build.md), [Cosmos.Build.Ilc](articles/dev/build/ilc-build.md): the build pipeline components.

## Cross-referencing

The API reference publishes a DocFX cross-reference map at [`/xrefmap.yml`](https://cosmosos.github.io/xrefmap.yml), covering every documented namespace, type and member. Another DocFX site consumes it by adding that URL to the `xref` list of its own `docfx.json`:

```json
{
  "build": {
    "xref": [ "https://cosmosos.github.io/xrefmap.yml" ]
  }
}
```

Cosmos types are then linked by UID instead of by URL, as `<xref:Cosmos.Kernel.System.Graphics.Canvas>` or its `@Cosmos.Kernel.System.Graphics.Canvas` shorthand. Each frozen release copy carries its own map beside it, at `/vX.Y.Z/xrefmap.yml` and `/latest/xrefmap.yml`.

## Resources
- [Cosmos Gen3: The NativeAOT Era and the End of IL2CPU?](https://valentin.bzh/posts/3)
- [NativeAOT Developer Workflow](https://github.com/dotnet/runtime/blob/main/docs/workflow/building/coreclr/nativeaot.md)
- [Native AOT Limitations](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/#limitations-of-native-aot-deployment)
