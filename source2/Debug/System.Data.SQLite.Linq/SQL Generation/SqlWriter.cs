//---------------------------------------------------------------------
// <copyright file="SqlWriter.cs" company="Microsoft">
//      Portions of this file copyright (c) Microsoft Corporation
//      and are released under the Microsoft Pulic License.  See
//      http://archive.msdn.microsoft.com/EFSampleProvider/Project/License.aspx
//      or License.txt for details.
//      All rights reserved.
// </copyright>
//---------------------------------------------------------------------

namespace System.Data.SQLite
{
  using System.IO;
  using System.Text;

    /// <summary>
  /// This extends StringWriter primarily to add the ability to add an indent
  /// to each line that is written out.
  /// </summary>
  class SqlWriter : StringWriter
  {
    // We start at -1, since the first select statement will increment it to 0.
    int indent = -1;
    /// <summary>
    /// The number of tabs to be added at the beginning of each new line.
    /// </summary>
    internal int Indent
    {
      get { return indent; }
      set { indent = value; }
    }

    bool atBeginningOfLine = true;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="b"></param>
    public SqlWriter(StringBuilder b)
      : base(b, System.Globalization.CultureInfo.InvariantCulture)
    {
    }

    /// <summary>
    /// Reset atBeginningofLine if we detect the newline string.
    /// <see cref="SqlBuilder.AppendLine"/>
    /// Add as many tabs as the value of indent if we are at the 
    /// beginning of a line.
    /// </summary>
    /// <param name="value"></param>
    public override void Write(string value)
    {
      if (value == "\r\n")
      {
        base.WriteLine();
        atBeginningOfLine = true;
      }
      else
      {
        if (atBeginningOfLine)
        {
          if (indent > 0)
          {
            base.Write(new string('\t', indent));
          }
          atBeginningOfLine = false;
        }
        base.Write(value);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void WriteLine()
    {
      base.WriteLine();
      atBeginningOfLine = true;
    }
  }
}
