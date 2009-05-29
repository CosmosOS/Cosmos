using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Cosmos.Build.Common;

namespace Cosmos.Build.MSBuild {
	
  public class BuildImage : Task {

		private Boolean buildFailed;

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

			buildKernelAssemblyPath = System.IO.Path.Combine(buildOutputPath, this.KernelAssembly);
			buildUseInternalAssembler = this.UseInternalAssembler;
			buildFramework = EnumValue.Parse(this.Framework, Cosmos.Build.Common.Framework.MicrosoftNET);
			buildTarget = EnumValue.Parse(this.BuildTarget, TargetHost.QEMU);

			Log.LogMessage(MessageImportance.High, "Output Path: {0}", buildOutputPath);
			Log.LogMessage(MessageImportance.High, "Kernal Assembly: {0}", buildKernelAssemblyPath);
			Log.LogMessage(MessageImportance.High, "Use Internal Assembler: {0}", buildUseInternalAssembler);
			Log.LogMessage(MessageImportance.High, "Framework: {0}", buildFramework);
			Log.LogMessage(MessageImportance.High, "Target: {0}", buildTarget);

			//System.Reflection.Assembly kernelAssembly;

			//var builtEvent = new System.Threading.AutoResetEvent(false);
			//var builder = new Builder();

			//builder.UseInternalAssembler = this.UseInternalAssembler;

			//builder.CompileCompleted += delegate { builtEvent.Set(); };
			//builder.LogMessage += delegate(Indy.IL2CPU.LogSeverityEnum aSeverity, string aMessage)
			//          {
			//    switch (aSeverity)
			//    {
			//      case Indy.IL2CPU.LogSeverityEnum.Informational:
			//        Log.LogMessage(aMessage);
			//        break;
			//      case Indy.IL2CPU.LogSeverityEnum.Warning:
			//        Log.LogWarning(aMessage);
			//        break;
			//      case Indy.IL2CPU.LogSeverityEnum.Error:
			//        Log.LogError(aMessage);
			//        this.buildFailed = true;
			//        break;
			//    }

			//          };

			//kernelAssembly = System.Reflection.Assembly.LoadFile(this.KernelAssemblyFile);
			//builder.TargetAssembly = kernelAssembly;

			//builder.BeginCompile(compileDebugMode, this.DebugComPort, this.GDB);
			//builtEvent.WaitOne();

			//if (this.buildFailed == true)
			//{
			//  builder.Assemble();
			//  builder.Link();
			//  builder.MakeISO();
			//}

			return (this.buildFailed == false);
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

	}
		
}
