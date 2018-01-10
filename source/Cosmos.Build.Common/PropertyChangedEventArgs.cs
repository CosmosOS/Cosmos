using System;

namespace Cosmos.Build.Common
{
    public class PropertyChangedEventArgs : EventArgs
    {
        public string PropertyName { get; }
        public string OldValue { get; }
        public string NewValue { get; }

        public PropertyChangedEventArgs(string propertyName, string oldValue, string newValue)
        {
            PropertyName = propertyName;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
