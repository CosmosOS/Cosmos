namespace Cosmos.Common
{
  using System;
  using Cosmos.Common.Extensions;

  /// <summary>
  /// Helper class for working with numbers.
  /// </summary>
  public static class NumberHelper
  {
    /// <summary>
    /// Write number to console.
    /// </summary>
    /// <param name="aValue">A value to print.</param>
    /// <param name="aZeroFill">A value indicating whether strarting zeros should be present.</param>
    public static void WriteNumber(uint aValue, bool aZeroFill)
    {
      if (aZeroFill)
      {
        Console.WriteLine("0x" + aValue.ToHex());
      }
      else
      {
        Console.WriteLine("0x" + aValue.ToHex().TrimStart('0'));
      }
    }
  }
}
