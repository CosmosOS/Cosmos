﻿using System;

namespace Cosmos.IL2CPU.Plugs
{
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public sealed class PlugAttribute : Attribute {
    public Type Target;
    public bool Inheritable = false;
    public string TargetName;
    public bool IsMonoOnly = false;
    public bool IsMicrosoftdotNETOnly = false;
  }
}