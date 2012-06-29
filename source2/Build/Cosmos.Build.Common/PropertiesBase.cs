using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Build.Common {
  public abstract class PropertiesBase {
    protected Dictionary<String, String> mPropTable = new Dictionary<String, String>();

    public Dictionary<String, String> GetProperties() {
      Dictionary<string, string> clonedTable = new Dictionary<String, String>();

      foreach (KeyValuePair<string, string> pair in mPropTable) { 
        clonedTable.Add(pair.Key, pair.Value); 
      }

      return clonedTable;
    }

    public void Reset() {
      mPropTable.Clear();
    }

    public void SetProperty(String name, String value) {
      if (mPropTable.ContainsKey(name) == false) { mPropTable.Add(name, value); } else { mPropTable[name] = value; }
    }

    public void SetProperty(String name, Object value) {
      SetProperty(name, value.ToString());
    }

    protected String GetProperty(String name) {
      return GetProperty(name, String.Empty);
    }

    protected T GetProperty<T>(String name, T @default) {
      T value = @default;
      if (mPropTable.ContainsKey(name) == true) {
        String stringValue = mPropTable[name];
        Type valueType = typeof(T);
        String valueTypeName = valueType.Name;

        if (valueType.IsEnum == true) {
          value = EnumValue.Parse(stringValue, @default);
        } else {
          if (valueTypeName == "String") {
            value = (T)((Object)stringValue);
          } else if ((valueTypeName == "Int16") || (valueTypeName == "Short")) {
            Int16 newValue;
            if (Int16.TryParse(stringValue, out newValue) == true) { value = (T)((Object)newValue); }

          } else if ((valueTypeName == "Int32") || (valueTypeName == "Integer")) {
            Int32 newValue;
            if (Int32.TryParse(stringValue, out newValue) == true) { value = (T)((Object)newValue); }

          } else if ((valueTypeName == "Int64") || (valueTypeName == "Long")) {
            Int32 newValue;
            if (Int32.TryParse(stringValue, out newValue) == true) { value = (T)((Object)newValue); }

          } else if (valueTypeName == "Boolean") {
            Boolean newValue;
            if (Boolean.TryParse(stringValue, out newValue) == true) { value = (T)((Object)newValue); }

          } else if ((valueTypeName == "UInt16") || (valueTypeName == "UShort")) {
            UInt16 newValue;
            if (UInt16.TryParse(stringValue, out newValue) == true) { value = (T)((Object)newValue); }

          } else if ((valueTypeName == "UInt32") || (valueTypeName == "UInteger")) {
            UInt32 newValue;
            if (UInt32.TryParse(stringValue, out newValue) == true) { value = (T)((Object)newValue); }

          } else if ((valueTypeName == "UInt64") || (valueTypeName == "ULong")) {
            UInt64 newValue;
            if (UInt64.TryParse(stringValue, out newValue) == true) { value = (T)((Object)newValue); }

          } else {
            throw new ArgumentException("Unsupported value type.", "T");
          }
        }
      }

      return value;
    }

  }
}
