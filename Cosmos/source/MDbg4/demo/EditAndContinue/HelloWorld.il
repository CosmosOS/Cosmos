//  Microsoft (R) .NET Framework IL Disassembler.  Version 1.2.30711.0
//  Copyright (C) Microsoft Corporation 1998-2003. All rights reserved.

// Metadata version: v1.2.30711
.assembly extern legacy library mscorlib
{
  auto



}
.assembly legacy library HelloWorld
{

  // --- The following custom attribute is added automatically, do not uncomment -------
  //  .custom instance void [mscorlib]System.Diagnostics.DebuggableAttribute::.ctor(bool,
  //                                                                                bool) = ( 01 00 00 01 00 00 ) 

  .hash algorithm 0x00008004
  .ver 0:0:0:0
}
.module HelloWorld.exe
// MVID: {9F4A2EAC-B19E-4D09-B988-5A948172960E}
.imagebase 0x00400000
.file alignment 0x00000200
.stackreserve 0x00100000
.subsystem 0x0003       // WINDOWS_CUI
.corflags 0x00000001    //  ILONLY
// Image base: 0x03EA0000


// =============== CLASS MEMBERS DECLARATION ===================

.class public auto ansi beforefieldinit HelloWorld
       extends [mscorlib]System.Object
{
  .method public hidebysig static void  Main() cil managed
  {
    .entrypoint
    // Code size       16 (0x10)
    .maxstack  8
    IL_0000:  ldstr      "Hello "
    IL_0005:  call       void [mscorlib]System.Console::Write(string)
    IL_000a:  call       void HelloWorld::PrintWorld()
    IL_000f:  ret
  } // end of method HelloWorld::Main

  .method private hidebysig static void  PrintWorld() cil managed
  {
    // Code size       11 (0xb)
    .maxstack  8
    IL_0000:  ldstr      "World"
    IL_0005:  call       void [mscorlib]System.Console::WriteLine(string)
    IL_000a:  ret
  } // end of method HelloWorld::PrintWorld

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ret
  } // end of method HelloWorld::.ctor

} // end of class HelloWorld


// =============================================================

//*********** DISASSEMBLY COMPLETE ***********************
// WARNING: Created Win32 resource file HelloWorld.res
