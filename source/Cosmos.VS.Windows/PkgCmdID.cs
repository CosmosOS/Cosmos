// PkgCmdID.cs
// MUST match PkgCmdID.h
using System;

namespace Cosmos.VS.Windows {
  static class PkgCmdIDList {
    public const uint cmdidCosmosAssembly = 0x101;
    public const uint cmdidCosmosRegisters = 0x102;
    public const uint cmdidCosmosStack = 0x103;
    public const uint cmdidCosmosInternal = 0x108;
    public const uint cmdidCosmosShowAll = 0x104;
    //
    public const uint cmdidAsmFilter = 0x0105;
    public const uint cmdidAsmStep = 0x0106;
    public const uint cmdidAsmCopy = 0x0107;
    //
    public const int AsmToolbar = 0x1001;
    public const uint AsmToolbarGroup = 0x1002;
  };
}