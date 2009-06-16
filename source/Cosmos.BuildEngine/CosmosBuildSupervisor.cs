using System;
using System.Reflection;
using System.Collections.Generic;

namespace Cosmos.BuildEngine
{
    public static class CosmosBuildSupervisor
    {
        private static CompilationResult BuildMethod(CosmosBuildPlatform Platform, MethodBase methodbase)
        {
            //- Make a CIL reader for methodbase
            //- Pass it to Platform.TranslateCIL()
            //- Take results and handle any errors appropriately
            //- Return results as CompilationResult

            return null; //TEMPOARY, so it will compile
        }

        private static void BuildAssembly(CosmosBuildPlatform Platform, String BuildDirectory, Assembly TargetAssembly)
        {
            //WARNING: Multiple instances running on different threads, so keep thread safe. No **direct** GUI interaction allowed.

            //The only problem will be types/structs defined in other assemblies. Those assemblies should be scanned for the implementation, if nessecary.

            //Each time a method is compiled, add it to a global list of compiled assemblies. Remove instances of this "missing method" from the uncompiled methods list.
            //Each time a method is referenced which is not marked as compiled, add it to a global list of uncompiled methods.
            //Each time a method is referenced as inline, compile it in place instead of with the CIL Call instruction.

            //Get all methods in TargetAssembly.
            //For each method:
            //  - If it has a plug locate the plug's assembly and then the plug itself. Do what you have to do, then "unload" the plug assembly. DO NOT compile the entire plug assembly. 
            //      - If the plug is Compiled CIL, BuildMethod it exactly how the original method would be.
            //      - If the plug is Emitted CIL, execute its CIL assembler and BuildMethod the result exactly how the original method would be.
            //      - If the plug is ASM, use it exactly how the original method would be.
            //          - If no proper implementation exists, shove a NotImplementedException in there and pray no one uses it. Mark it as compiled.
            //  - Otherwise, BuildMethod it.
            //  - Write the results to two unique files for that method, one for data and one for code. See below.

            //File Format for Plugs:
            //  Filename: asm/cache/[original method assembly name]~~[original method assembly version]/Plugs/[plug assembly name]~~[plug assembly verson]/[MethodSignature].[data|code].asm
            //  File Layout: Exactly how it should appear in the master file(s).
            //File Format for Original Methods:
            //  Filename: asm/cache/[assembly name]~~[assembly version]/[MethodSignature].[data|code].asm
            //  File Layout: Exactly how it should appear in the master file(s).

            //Mark the thread as finished and return. Report any errors along the way to the Supervisor.
            //Scan the method as it is compiled and add any nesecarry dependances to a list for that method.
            //If the method is completly and always needed, but is not nessecarily referenced, add it to a different list.
        }

        public static bool Build<Platform>
            (
                String BuildDirectory, IEnumerable<BuildOption> BuildOptions, IEnumerable<BuildOption> PlatformOptions,
                CosmosBuildAssembler Assembler, CosmosBuildTarget Target,
                IEnumerable<BuildOption> AssemblerOptions, IEnumerable<BuildOption> TargetOptions
            )
            where Platform : CosmosBuildPlatform, new()
        {
            Platform platform = new Platform();
            String BuildPath = BuildDirectory + platform.GetDisplayName() + "_" + platform.GetPostfixForOptions() + "\\";

            //If an error occurs anyware, halt build, empty entire cache, pass details to GUI, and return false.

            //- Locate All Cosmos Assemblies (Ignore Plug and Builder Assemblies)
            //- Create a thread for each Assembly and run BuildAssembly (with a unique new Platform) on it if the assembly needs to be built
            //  (i.e. the proper versioned folder exists and all plugs are properly versioned).
            //- Wait for all threads to complete.
            //- Search for any "Uncompiled Methods" and attempt to resolve them with the dynamic method system.
            //  These dynamic methods may be cached (as "asm/cache/dynamc/[unique name].[data|code].asm"), but by their nature some may not be cacheable.
            //- If any remain, shove a NotImplementedException in there and pray no one uses it.
            //- Scan Output Directory for all nessecary data and code sections and assemble them into (a) master file(s). (as "asm/master_[name].asm")
            //     - Scanning should be done so only nessecary code is included. Cached assemblies will need to be scanned as well, but not recompiled.
            //     - Remember to include PreDataCode,PostDataCode,PostCodeCode in master file(s).
            //- Pass master file(s) to assembler.   (assemble as "bin/master_[name].bin")
            //- Pass assembled binaries to target.
            //- Act upon the target results.

            return true;
        }

        public static IEnumerable<BuildOption> GetBuildOptions()
        {
        }
    }
}