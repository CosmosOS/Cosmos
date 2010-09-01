using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using Cosmos.System;

namespace Cosmos.System.Plugs.System
{
    [Plug(Target = typeof(global::System.Math))]
    public class MathImpl
    {
        public const double PI = 3.14159265358979;
        public const double E = 2.71828182845905;

        public static double Abs(double value)
        {
            double xResult;
            if (value < 0)
            {
                xResult = value - (2 * value);
            }
            else
            {
                xResult = value;
            }
            return xResult;
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
            int xResult;
            if (value < 0)
            {
                xResult = value - (2 * value);
            }
            else
            {
                xResult = value;
            }
            return xResult;
        }

        public static double Pow(double x, double y)
        {
            double xResult = x;
            if (y == 0)
            {
                xResult = 1;
            }
            else if (y == 1)
            {
                xResult = x;
            }
            else
            {
                for (int i = 2; i <= y; i++)
                {
                    xResult = xResult * x;
                }
            }
            return xResult;
        }
    }
}