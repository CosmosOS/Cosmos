using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kudzu.BreakpointsKernel;

namespace Playground.Kudzu.BreakpointsKernel
{
  public class ListTest: Test
  {
    public override void Run()
    {
      var xList = new List<int>();
      var xList2 = new List<int>();
      Console.WriteLine("List.Capacity = " + xList.Capacity);
      Console.WriteLine("List2.Capacity = " + xList2.Capacity);
      Console.WriteLine("List.Count = " + xList.Count);
      Console.WriteLine("List2.Count = " + xList2.Count);
      xList.Add(2);
      xList2.Add(2);
      Console.WriteLine("Item added");
      Console.WriteLine("List.Capacity = " + xList.Capacity);
      Console.WriteLine("List2.Capacity = " + xList2.Capacity);
      Console.WriteLine("List.Count = " + xList.Count);
      Console.WriteLine("List2.Count = " + xList2.Count);
      xList.Clear();
      xList2.Clear();
      Console.WriteLine("Lists cleared");
      Console.WriteLine("List.Capacity = " + xList.Capacity);
      Console.WriteLine("List2.Capacity = " + xList2.Capacity);
      Console.WriteLine("List.Count = " + xList.Count);
      Console.WriteLine("List2.Count = " + xList2.Count);
    }
  }
}
