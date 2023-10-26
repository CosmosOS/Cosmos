<p align="center">

  <img src="https://user-images.githubusercontent.com/63316499/89792973-43587480-daf3-11ea-99d6-82f89dd2ffc3.png" width="25%" />

</p>

<p align="center">

  <a href="https://ci.appveyor.com/api/projects/status/kust7g5dlnykhkaf/branch/master">
    <img src="https://ci.appveyor.com/api/projects/status/kust7g5dlnykhkaf/branch/master?svg=true" />
  </a>

  <img src="https://img.shields.io/github/languages/code-size/CosmosOS/Cosmos" />
  <img src="https://img.shields.io/github/downloads/CosmosOS/Cosmos/total" />

  <a href="https://github.com/CosmosOS/Cosmos/releases/latest">
    <img src="https://img.shields.io/github/v/release/CosmosOS/Cosmos" />
  </a>

  <a href="https://github.com/CosmosOS/Cosmos/blob/master/LICENSE.txt">
    <img src="https://img.shields.io/github/license/CosmosOS/Cosmos" />
  </a>
  
  <a href="https://discord.com/invite/kwtBwv6jhD">
    <img src="https://img.shields.io/discord/833970409337913344" />
  </a>

</p>

<hr/>

Cosmos (C# Open Source Managed Operating System) is an operating system development kit which uses .NET, alongside the custom IL2CPU compiler to convert (most) C# code into a working bare-metal OS.
Despite C# in the name, any .NET-based language can be used, which includes: VB.NET, IronPython, F# and more. Cosmos itself and its kernel routines are primarily written in C#, and thus the Cosmos name.

In a project, Cosmos can be thought of as a compiler and a sort-of standard library for a project. It gives the user access to often hard to find or otherwise difficult to understand tools.

<hr/>

## Features

The following is a non-exhaustive list of features that Cosmos offers:

- Low level assembly access and pointer memory control
- A basic (and unstable at the moment) filesystem
- Most features found in the .NET core library
- A CPU/FPU accelerated math library
- A basic graphics interface
- A basic network interface
- A basic audio interface

> **Note**
> Use [embeded resources](https://cosmosos.github.io/articles/Kernel/ManifestResouceStream.html) instead of the VFS for now for assets.

## Setting it up

Cosmos has an article [here](https://cosmosos.github.io/install.html) on how to do that.

## Documentation

The Cosmos documentation can be found [here](https://cosmosos.github.io/api/Cosmos.Build.Common.html).

If you still have any questions on how to use Cosmos, you can open a [discussion](https://github.com/CosmosOS/Cosmos/discussions) or you can join the [Discord server](https://discord.com/invite/kwtBwv6jhD)!

The devkit changelog can be found [here](https://cosmosos.github.io/articles/Changelog.html).

## Reporting an issue

If you think you found a bug in Cosmos, please check existing [issues](https://github.com/CosmosOS/Cosmos/issues) first before opening a new one. Do **not** open an issue if you need help with something in Cosmos that is not a bug, if you don't know how to code it's not a Cosmos issue for example.
