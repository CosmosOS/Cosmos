using System;

namespace Cosmos.IL2CPU.Plugs
{
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public sealed class PlugAttribute : Attribute 
  {
    public Type Target;
    public bool Inheritable = false;
    public string TargetName;
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