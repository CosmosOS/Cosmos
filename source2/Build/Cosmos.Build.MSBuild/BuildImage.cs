using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
//using Cosmos.Compiler.Builder;

namespace Cosmos.Build.MSBuild {
	
  public class BuildImage : Task {

		private Boolean buildFailed;

		public override bool Execute() {
			buildFailed = false;
      Log.LogMessage(MessageImportance.High, "Building Cosmos System Image");
      return true;

			this.KernelAssemblyFile = (new System.IO.FileInfo(this.KernelAssemblyFile)).FullName;

			System.Reflection.Assembly kernelAssembly;
			//Indy.IL2CPU.DebugMode compileDebugMode;
			var builtEvent = new System.Threading.AutoResetEvent(false);
			//var builder = new Builder();

			if (String.IsNullOrEmpty(this.BuildPath) == false)
			{
				this.BuildPath = (new System.IO.DirectoryInfo(this.BuildPath)).FullName;
				//builder.BuildPath = this.BuildPath;
			}
			//builder.UseInternalAssembler = this.UseInternalAssembler;
			
			//compileDebugMode = Indy.IL2CPU.DebugMode.None;
			if (String.IsNullOrEmpty(this.DebugMode) == false)
			{
				//if( Enum.IsDefined(typeof(Indy.IL2CPU.DebugMode), this.DebugMode) == true)
				//{
				//	compileDebugMode = (Indy.IL2CPU.DebugMode)Enum.Parse(typeof(Indy.IL2CPU.DebugMode), this.DebugMode, true);
				//}else{
				//	Log.LogWarning("Unknown Cosmos debug mode, defaulted to none.");
				//}
			}

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

			return this.buildFailed;
		}

		[Required]
		public string KernelAssemblyFile
		{ get; set; }

		public string BuildPath
		{ get; set; }

		public Boolean UseInternalAssembler
		{ get; set; }

		public string DebugMode
		{ get; set; }

		public byte DebugComPort
		{ get; set; }

		public Boolean GDB
		{ get; set; }

	}
		
}
