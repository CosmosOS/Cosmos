using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Cosmos.Build.Common;
using Cosmos.Compiler.Builder;
using Cosmos.IL2CPU;
using System.IO;
using System.Reflection;

namespace Cosmos.Build.MSBuild {

    public class BuildImage : AppDomainIsolatedTask
    {

		private Boolean buildFailed;
        private static string mBasePath;

		public override bool Execute()
		{
            buildFailed = false;

            Log.LogMessage(MessageImportance.High, "Building Cosmos System Image");

            String buildOutputPath;
            String buildKernelAssemblyPath;
            Boolean buildUseInternalAssembler;
            TargetHost buildTarget;
            Framework buildFramework;

            if (System.IO.Path.IsPathRooted(this.OutputPath) == false)
            { buildOutputPath = (new System.IO.DirectoryInfo(this.OutputPath)).FullName; }
            else
            { buildOutputPath = this.OutputPath; }
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            mBasePath = buildOutputPath;
            
            buildKernelAssemblyPath = System.IO.Path.Combine(buildOutputPath, this.KernelAssembly);
            buildUseInternalAssembler = this.UseInternalAssembler;
            buildFramework = EnumValue.Parse(this.Framework, Cosmos.Build.Common.Framework.MicrosoftNET);
            buildTarget = EnumValue.Parse(this.BuildTarget, TargetHost.QEMU);

            Log.LogMessage(MessageImportance.High, "Output Path: {0}", buildOutputPath);
            Log.LogMessage(MessageImportance.High, "Kernal Assembly: {0}", buildKernelAssemblyPath);
            Log.LogMessage(MessageImportance.High, "Use Internal Assembler: {0}", buildUseInternalAssembler);
            Log.LogMessage(MessageImportance.High, "Framework: {0}", buildFramework);
            Log.LogMessage(MessageImportance.High, "Target: {0}", buildTarget);

            System.Reflection.Assembly kernelAssembly;

            var builtEvent = new System.Threading.AutoResetEvent(false);
            var builder = new Builder();
            var xOptions = BuildOptions.Load();
            xOptions.Target = "ISO";
            xOptions.DebugMode = DebugMode.Source;
            //builder.UseInternalAssembler = this.UseInternalAssembler;
            builder.BuildCompleted += delegate { builtEvent.Set(); };
            builder.LogMessage += delegate(LogSeverityEnum aSeverity, string aMessage)
                      {
                          switch (aSeverity)
                          {
                              case LogSeverityEnum.Informational:
                                  Log.LogMessage(aMessage);
                                  break;
                              case LogSeverityEnum.Warning:
                                  Log.LogWarning(aMessage);
                                  break;
                              case LogSeverityEnum.Error:
                                  Log.LogError(aMessage);
                                  this.buildFailed = true;
                                  break;
                          }

                      };

            kernelAssembly = System.Reflection.Assembly.LoadFile(Path.Combine(buildOutputPath, this.KernelAssembly));
            builder.TargetAssembly = kernelAssembly;

            builder.BeginCompile(xOptions);
            builtEvent.WaitOne();
            if (!builder.HasErrors)
            {
                File.Copy(Path.Combine(builder.BuildPath, "Cosmos.iso"), Path.Combine(OutputPath, Path.GetFileNameWithoutExtension(KernelAssembly) + ".iso"), true);
            }
            return (this.buildFailed == false) && !builder.HasErrors;
            //Log.LogWarning("Not rebuilding image");
            //return true;
		}

        System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            foreach (var xAssembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (xAssembly.FullName == args.Name)
                {
                    return xAssembly;
                }
            }
            var xAsmName = args.Name;
            if (xAsmName.Contains(","))
            {
                xAsmName = xAsmName.Substring(0, xAsmName.IndexOf(','));
            }
            var xBasePath = Path.Combine(CosmosDir, "Build\\Tools\\Cosmos.Hardware");
            if(File.Exists(Path.Combine(xBasePath, xAsmName + ".dll"))){
                return Assembly.LoadFile(Path.Combine(xBasePath, xAsmName + ".dll"));
            }
            if(File.Exists(Path.Combine(xBasePath, xAsmName + ".exe"))){
                return Assembly.LoadFile(Path.Combine(xBasePath, xAsmName + ".exe"));
            }
            xBasePath = mBasePath;
            if (File.Exists(Path.Combine(xBasePath, xAsmName + ".dll")))
            {
                return Assembly.LoadFile(Path.Combine(xBasePath, xAsmName + ".dll"));
            }
            if (File.Exists(Path.Combine(xBasePath, xAsmName + ".exe")))
            {
                return Assembly.LoadFile(Path.Combine(xBasePath, xAsmName + ".exe"));
            }

            return null;
        }

		[Required]
		public string KernelAssembly
		{ get; set; }

		[Required]
		public string OutputPath
		{ get; set; }

		public Boolean UseInternalAssembler
		{ get; set; }

		public string BuildTarget
		{ get; set; }

		public string Framework
		{ get; set; }

        [Required]
        public string CosmosDir
        {
            get;
            set;
        }

	}
		
}
