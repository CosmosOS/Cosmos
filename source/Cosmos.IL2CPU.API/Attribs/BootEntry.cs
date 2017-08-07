using System;

namespace Cosmos.IL2CPU.API.Attribs
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class BootEntry : Attribute
    {
        public int? EntryIndex { get; }

        public BootEntry(bool aDefault = true, int aEntryIndex = 0)
        {
            if (aDefault)
            {
                EntryIndex = null;
            }
            else
            {
                EntryIndex = aEntryIndex;
            }
        }
    }
}
