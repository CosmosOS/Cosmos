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

		// TODO Abs decimal
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

		/* should work from ms .net
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
        }*/
        #endregion

		// TODO Max decimal
        #region Max
    
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

        #endregion

		//TODO Min decimal
        #region Min

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

        #endregion

        #region Pow
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
        #endregion

        #region Sin
        public static double Sin(double a)
        { // should be using assembler instruction
            bool signSwitch = false;
            double result = 0;

            //TO radians
            double radians = a;// *(Math.PI / 180);

            if (radians > Math.PI)
            {
                radians = radians - Math.PI;
                signSwitch = true;
            }
            else if (a > Math.PI / 2)
            {
                radians = radians - Math.PI;
                signSwitch = true;
            }

            //Temp function to increase precision make more factorial calculations
            result = (radians) - (Math.Pow(radians, 3) / Factorial(3));
            result += (Math.Pow(radians, 5) / Factorial(5)) - (Math.Pow(radians, 7) / Factorial(7)) + (Math.Pow(radians, 9) / Factorial(9));

            /* USE WHEN Modulus Works
             * int sign = 0;
            for (int i = 3; i < 19; i += 2)
            {
                if (sign % 2 == 0)
                    result += -Math.Pow(radians, i) / fact(i);
                else
                    result += Math.Pow(radiansa, i) / fact(i);
                sign++;
            }*/

            if (signSwitch)
                return result * -1;
            else
                return result;
        }
        #endregion

        #region Cos
        public static double Cos(double a)
        {
            //Cos(x) = Sin(90degrees - radians)
            return Sin((Math.PI / 2) - a);
        }
        #endregion

        #region Tan
        public static double Tan(double a)
        {
            return Sin(a) / Cos(a);
        }
        #endregion

        #region Factorial (only used in Sin(), not plug )
        public static int Factorial(int n)
        {
            if (n == 0)
                return 1;
            else
                return n * Factorial(n - 1);
        }
        #endregion

        #region Ceiling
		public static double Ceiling(double a)
		{ // should be using assembler for bigger values than int or long max
			if (a == Double.NaN || a == Double.NegativeInfinity || a == Double.PositiveInfinity)
				return a;
			int i = (a - (int)a > 0) ? (int)(a + 1) : (int)a;
			return i;
		}
        #endregion

        #region Floor
		public static double Floor(double a)
		{ // should be using assembler for bigger values than int or long max
			if (a == Double.NaN || a == Double.NegativeInfinity || a == Double.PositiveInfinity)
				return a;
			int i = (a - (int)a < 0) ? (int)(a - 1) : (int)a;
			return i;
		}
        #endregion
    }
}