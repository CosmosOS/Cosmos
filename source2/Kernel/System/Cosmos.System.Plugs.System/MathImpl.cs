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

        #region Abs
        public static double Abs(double value)
        {
            if (value < 0)
            {
                return -value;
            }
            else
            {
                return value;
            }
        }

        public static float Abs(float value)
        {
            if (value < 0)
            {
                return -value;
            }
            else
            {
                return value;
            }
        }

        public static long Abs(long value)
        {
            if (value < 0)
            {
                return -value;
            }
            else
            {
                return value;
            }
        }

        public static int Abs(int value)
        {
            if (value < 0)
            {
                return -value;
            }
            else
            {
                return value;
            }
        }
        #endregion

        #region Max
        public static byte Max(byte val1, byte val2)
        {
            if (val1 > val2)
            {
                return val1;
            }
            else
            {
                return val2;
            }
        }

        //public static decimal Max(decimal val1, decimal val2)
        //{
        //    if (val1 > val2)
        //    {
        //        return val1;
        //    }
        //    else
        //    {
        //        return val2;
        //    }
        //}

        public static double Max(double val1, double val2)
        {
            if (val1 > val2)
            {
                return val1;
            }
            else
            {
                return val2;
            }
        }

        public static short Max(short val1, short val2)
        {
            if (val1 > val2)
            {
                return val1;
            }
            else
            {
                return val2;
            }
        }

        public static int Max(int val1, int val2)
        {
            if (val1 > val2)
            {
                return val1;
            }
            else
            {
                return val2;
            }
        }

        public static long Max(long val1, long val2)
        {
            if (val1 > val2)
            {
                return val1;
            }
            else
            {
                return val2;
            }
        }

        public static float Max(float val1, float val2)
        {
            if (val1 > val2)
            {
                return val1;
            }
            else
            {
                return val2;
            }
        }
        #endregion

        #region Min
        public static byte Min(byte val1, byte val2)
        {
            if (val1 < val2)
            {
                return val1;
            }
            else
            {
                return val2;
            }
        }

        //public static decimal Min(decimal val1, decimal val2)
        //{
        //    if (val1 < val2)
        //    {
        //        return val1;
        //    }
        //    else
        //    {
        //        return val2;
        //    }
        //}

        public static double Min(double val1, double val2)
        {
            if (val1 < val2)
            {
                return val1;
            }
            else
            {
                return val2;
            }
        }

        public static short Min(short val1, short val2)
        {
            if (val1 < val2)
            {
                return val1;
            }
            else
            {
                return val2;
            }
        }

        public static int Min(int val1, int val2)
        {
            if (val1 < val2)
            {
                return val1;
            }
            else
            {
                return val2;
            }
        }

        public static long Min(long val1, long val2)
        {
            if (val1 < val2)
            {
                return val1;
            }
            else
            {
                return val2;
            }
        }

        public static float Min(float val1, float val2)
        {
            if (val1 < val2)
            {
                return val1;
            }
            else
            {
                return val2;
            }
        }
        #endregion

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