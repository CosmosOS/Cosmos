using System;

namespace Cosmos.IL2CPU.API.Attribs {
  [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
  public sealed class BootEntry : Attribute {
    public int? EntryIndex { get; }

    public BootEntry(int aEntryIndex = -1) {
      EntryIndex = aEntryIndex;
      if (aEntryIndex == -1) {
        EntryIndex = null;
      } 
    }
  }
}
