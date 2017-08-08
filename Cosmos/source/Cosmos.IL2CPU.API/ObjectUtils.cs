using System;
using System.Reflection;

namespace Cosmos.IL2CPU.API
{
  public static class ObjectUtils
  {
    /// <summary>
    ///		<para>
    ///			The object first stores any metadata involved. (Most likely containing a reference to the
    ///			object type). This is the number of bytes.
    ///		</para>
    ///		<para>
    ///			The first 4 bytes are the reference to the type information of the instance,
    ///         the second 4 bytes are the <see cref="InstanceTypeEnum"/> value.
    ///         For arrays, there are 4 following bytes containing the element count,
    ///         for objects, the amount of reference fields.
    ///         For arrays, next 4 bytes containing the element size.
    ///		</para>
    /// </summary>
    public const int FieldDataOffset = 12;

    public enum InstanceTypeEnum : uint
    {
      NormalObject = 1,

      Array = 2,

      BoxedValueType = 3,

      StaticEmbeddedObject = 0x80000001,

      StaticEmbeddedArray = 0x80000002
    }

    public static bool IsDelegate(Type aType)
    {
      if (aType.FullName == "System.Object")
      {
        return false;
      }
      if (aType.GetTypeInfo().BaseType.FullName == "System.Delegate")
      {
        return true;
      }
      if (aType.GetTypeInfo().BaseType.FullName == "System.Object")
      {
        return false;
      }
      return IsDelegate(aType.GetTypeInfo().BaseType);
    }

    public static bool IsArray(Type aType)
    {
      if (aType.FullName == "System.Object")
      {
        return false;
      }
      if (aType.GetTypeInfo().BaseType.FullName == "System.Array")
      {
        return true;
      }
      if (aType.GetTypeInfo().BaseType.FullName == "System.Object")
      {
        return false;
      }
      return IsArray(aType.GetTypeInfo().BaseType);
    }
  }
}
