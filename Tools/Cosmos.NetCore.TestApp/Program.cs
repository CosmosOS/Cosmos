using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyModel;

namespace Cosmos.NetCore.TestApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var xType = Type.GetType("System.Object");

            var ctx = DependencyContext.Default;

            Console.WriteLine("--- Native libraries ---");
            foreach (var r in ctx.GetDefaultNativeAssets())
            {
                        Console.WriteLine($"    {r}");


                }
                Console.WriteLine();


            Console.ReadKey();
        }
    }
}
