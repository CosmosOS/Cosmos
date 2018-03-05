using System;
using System.Collections.Generic;

namespace Cosmos.Build.Common
{
    public abstract class PropertiesBase
    {
        protected Dictionary<string, string> mPropTable = new Dictionary<string, string>();

        public event EventHandler<PropertyChangingEventArgs> PropertyChanging;
        public event EventHandler<PropertyChangedEventArgs> PropertyChanged;

        public bool IsDirty { get; private set; }

        public Dictionary<string, string> GetProperties()
        {
            Dictionary<string, string> clonedTable = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> pair in mPropTable)
            {
                clonedTable.Add(pair.Key, pair.Value);
            }

            return clonedTable;
        }

        /// <summary>
        /// Gets array of project names which are project independent.
        /// </summary>
        public abstract string[] ProjectIndependentProperties { get; }

        public void Reset()
        {
            mPropTable.Clear();
            IsDirty = false;
        }

        public void SetProperty(string name, string value)
        {
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(name));

            mPropTable.TryGetValue(name, out var xOldValue);
            mPropTable[name] = value;
            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name, xOldValue, value));
            IsDirty = true;
        }

        public void SetProperty(string name, Object value)
        {
            SetProperty(name, value.ToString());
        }

        public string GetProperty(string name)
        {
            return GetProperty(name, string.Empty);
        }

        /// <summary>
        /// Get string value of the property.
        /// </summary>
        /// <param name="name">Name of the property.</param>
        /// <param name="default">Default value for the property.</param>
        /// <returns>Vaue of the property with given name.</returns>
        public string GetProperty(string name, string @default)
        {
            string value = @default;
            if (mPropTable.ContainsKey(name) == true)
            {
                value = mPropTable[name];
            }

            return value;
        }

        /// <summary>
        /// Gets typed value of the property.
        /// </summary>
        /// <typeparam name="T">Get property type.</typeparam>
        /// <param name="name">Get name of the property.</param>
        /// <param name="default">Default value for the proeprty.</param>
        /// <returns>Value of the property with given name.</returns>
        public T GetProperty<T>(string name, T @default)
            where T : struct
        {
            T value = @default;
            if (mPropTable.ContainsKey(name) == true)
            {
                string stringValue = mPropTable[name];
                Type valueType = typeof(T);
                string valueTypeName = valueType.Name;

                if (valueType.IsEnum)
                {
                    value = EnumValue.Parse(stringValue, @default);
                }
                else
                {
                    if ((valueTypeName == "Int16") || (valueTypeName == "Short"))
                    {
                        Int16 newValue;
                        if (Int16.TryParse(stringValue, out newValue) == true) { value = (T)((Object)newValue); }

                    }
                    else if ((valueTypeName == "Int32") || (valueTypeName == "Integer"))
                    {
                        Int32 newValue;
                        if (Int32.TryParse(stringValue, out newValue) == true) { value = (T)((Object)newValue); }

                    }
                    else if ((valueTypeName == "Int64") || (valueTypeName == "Long"))
                    {
                        Int32 newValue;
                        if (Int32.TryParse(stringValue, out newValue) == true) { value = (T)((Object)newValue); }

                    }
                    else if (valueTypeName == "Boolean")
                    {
                        Boolean newValue;
                        if (Boolean.TryParse(stringValue, out newValue) == true) { value = (T)((Object)newValue); }

                    }
                    else if ((valueTypeName == "UInt16") || (valueTypeName == "UShort"))
                    {
                        UInt16 newValue;
                        if (UInt16.TryParse(stringValue, out newValue) == true) { value = (T)((Object)newValue); }

                    }
                    else if ((valueTypeName == "UInt32") || (valueTypeName == "UInteger"))
                    {
                        UInt32 newValue;
                        if (UInt32.TryParse(stringValue, out newValue) == true) { value = (T)((Object)newValue); }

                    }
                    else if ((valueTypeName == "UInt64") || (valueTypeName == "ULong"))
                    {
                        UInt64 newValue;
                        if (UInt64.TryParse(stringValue, out newValue) == true) { value = (T)((Object)newValue); }

                    }
                    else
                    {
                        throw new ArgumentException("Unsupported value type.", "T");
                    }
                }
            }

            return value;
        }

    }
}
