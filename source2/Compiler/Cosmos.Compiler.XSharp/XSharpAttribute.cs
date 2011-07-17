using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.XSharp {

  [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
  public class XSharpAttribute : Attribute {
    public bool IsInteruptHandler { get; set; }
    public bool PreserveStack { get; set; }
  }

}
