// Guids.cs
// MUST match guids.h - There is no such file???? I think its referencing old VS2005 style, but vsct is used now instead.
using System;

namespace Cosmos.VS.Windows {
  static class GuidList {
    public const string guidCosmos_VS_WindowsPkgString = "a82b45e9-2a89-43bd-925d-c7f0edd212aa";
    // What is guidToolWindowPersistanceString used for?
    public const string guidToolWindowPersistanceString = "f019fb29-c2c2-4d27-9abf-739533c939be";
    public const string guidAsmToolbarCmdSetString = "A875EA24-689A-4EAB-B9C2-DAC3EACB9501";
    public const string guidCosmosMenuString = "3247C3E4-34B8-4DB0-8748-AD62495A5222";

    public static readonly Guid guidCosmosMenu = new Guid(guidCosmosMenuString);
    public static readonly Guid guidAsmToolbarCmdSet = new Guid(guidAsmToolbarCmdSetString);
  };
}