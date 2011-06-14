using System;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Orvid.Graphics
{
    [Plug(Target = typeof(global::System.Math))]
    class MathImpl
    {
        [PlugMethod()]
        public static double Sin(double m)
        {
            int x = 1;
            for (int i = 3; i < 13; i = i + 2)
            {
                double factorial = 1;
                for (int i2 = i; i2 > 0; i2--)
                {
                    factorial = factorial * i2;
                }

                m = m + Math.Pow(-1.0, x) * (Math.Pow(m, i)) / factorial;

                x++;
            }
            return m;
        }

        [PlugMethod()]
        public static double Cos(double x)
        {
            x = Math.Abs((x + Math.PI) % (2 * Math.PI) - Math.PI);
            const double tf = 1.0 / 24.0;
            const double vtz = -1.0 / 720.0;
            const double fzhtz = 1.0 / 40320.0;
            const double fukit = -1.0 / 3628800.0;
            double p = x * x;
            return 1 + p * (-0.5 + p * (tf + p * (vtz + p * (fzhtz + p * fukit))));
        }

        [PlugMethod()]
        public static double Floor(double d)
        {
            String str = d.ToString();
            return (double)Int32.Parse(str.Substring(0, str.IndexOf('.')));
        }
    }
}
