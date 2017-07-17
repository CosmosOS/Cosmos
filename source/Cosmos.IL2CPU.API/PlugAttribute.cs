using System;

namespace Cosmos.IL2CPU.API
{
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public sealed class PlugAttribute : Attribute 
  {
      public PlugAttribute()
      {
      }
      public PlugAttribute(Type target)
      {
          if (null == target) { throw new ArgumentNullException(); }
          Target = target;
          return;
      }

      public PlugAttribute(string targetName)
      {
          if (string.IsNullOrEmpty(targetName)) { throw new ArgumentNullException(); }
          TargetName = targetName;
          return;
      }

      public Type Target { get; set; }

      public string TargetName { get; set; }

      public bool IsOptional
      {
          get;
          set;
      }
      
      public bool Inheritable = false;
      public bool IsMonoOnly = false;
      public bool IsMicrosoftdotNETOnly = false;
      public FrameworkVersion TargetFramework = FrameworkVersion.v4_0;
  }
  
  [Flags]
  public enum FrameworkVersion
  {
	v1_0,
	v1_1,
	v2_0,
	v3_0,
	v3_5,
	v3_5_Sp1,
	v4_0
  }
}