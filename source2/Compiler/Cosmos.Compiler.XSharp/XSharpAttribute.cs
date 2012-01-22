using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler.XSharp {

  [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
  public sealed class XSharpAttribute : Attribute {
    public bool IsInteruptHandler { get; set; }
    public bool PreserveStack { get; set; }
  }

}
