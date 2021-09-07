using System;

namespace IL2CPU.API
{
  [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
  public sealed class PlugMethodAttribute: Attribute {
    public string Signature = null;
    public bool Enabled = true;
    public Type Assembler = null;
    public bool IsMonoOnly = false;
    public bool IsMicrosoftdotNETOnly = false;
    
    public bool IsWildcard = false;
    public bool WildcardMatchParameters = false;
  }
}