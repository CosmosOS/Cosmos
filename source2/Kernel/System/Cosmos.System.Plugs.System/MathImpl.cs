using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using Cosmos.System;

namespace Cosmos.System.Plugs.System.System
{
    [Plug(Target = typeof(global::System.Math))]
    public class MathImpl
    {
        public const double PI = 3.14159265358979;
        public const double E = 2.71828182845905;

        public static double Abs(double value)
        {
            if (value < 0)            
                return -value;
            
            else            
                return value;
        }
        
        //public static float Abs(float value)
        //{
        //  float xResult;
        //  if (value < 0)
        //  {
        //    xResult = value - (2 * value);
        //  }
        //  else
        //  {
        //    xResult = value;
        //  }
        //  return xResult;
        //}

        //public static long Abs(long value)
        //{
        //  long xResult;
        //  if (value < 0)
        //  {
        //    xResult = value - (2 * value);
        //  }
        //  else
        //  {
        //    xResult = value;
        //  }
        //  return xResult;
        //}

        public static int Abs(int value)
        {
            if (value < 0)
                return -value;

            else
                return value;
        }

        public static double Pow(double x, double y)
        {            
            if (y == 0)
            {
                return 1;
            }
            else if (y == 1)
            {
                return x;
            }
            else
            {
                double xResult = x;

                for (int i = 2; i <= y; i++)
                {
                    xResult = xResult * x;
                }

                return xResult;
            }                        
        }
    }
}