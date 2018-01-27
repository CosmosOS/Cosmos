using System;

namespace Cosmos.Build.Common
{
    public class PropertyChangingEventArgs : EventArgs
    {
        public string PropertyName { get; }

        public PropertyChangingEventArgs(string propertyName)
        {
            PropertyName = propertyName;
        }
    }
}
