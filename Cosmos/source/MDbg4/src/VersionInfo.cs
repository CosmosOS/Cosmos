[assembly:System.Reflection.AssemblyTitle("Managed Debugger Sample")] 
[assembly:System.Reflection.AssemblyCompany("Microsoft Corporation")] 
[assembly:System.Reflection.AssemblyCopyright("Copyright © Microsoft Corporation.  All rights reserved.")] 
[assembly:System.Reflection.AssemblyTrademark("Microsoft® is a registered trademark of Microsoft Corporation.")] 
[assembly:System.Reflection.AssemblyVersion("2.1.0")] 
/* 
 * *******************
 * Maintenance History
 * *******************
 * 
 *  - Version 2.1.0  
 *      - More advanced debug event control and logging is now available via "ca" and "log" commands.
 *      - Breaking change in IMdbgIO.WriteOutput. This no longer includes a new line.
 *      - IronPython extension added.
 *      - Managed wrappers for native debug APIs added.
 *      - Pdb to Xml conversion tool added.
 * 
 */