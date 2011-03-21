using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;

namespace SSchockePlayground
{
    public class Kernel : Sys.Kernel
    {
        //public class DictIntComparer : IEqualityComparer<int>
        //{
        //    public bool Equals(int x, int y)
        //    {
        //        return x == y;
        //    }

        //    public int GetHashCode(int obj)
        //    {
        //        return obj;
        //    }
        //}

        //private Dictionary<int, string> testDict;
        //private DictIntComparer testDictComparer;

        public class IntMap<TValue> : Map<int, TValue>
        {
            protected override bool keysEqual(int key1, int key2)
            {
                return (key1 == key2);
            }
        }
        public class StringMap<TValue> : Map<string, TValue>
        {
            protected override bool keysEqual(string key1, string key2)
            {
                return (key1 == key2);
            }
        }

        public float Pow(float x, float y)
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
                float xResult = x;

                for (int i = 2; i <= y; i++)
                {
                    xResult = xResult * x;
                }

                return xResult;
            }
        }
        private float Sqrt(float d)
        {
            if ((d < 0) || (float.IsNaN(d) == true) || (float.IsNegativeInfinity(d) == true))
            {
                return float.NaN;
            }
            if (float.IsPositiveInfinity(d) == true)
            {
                return float.PositiveInfinity;
            }
            if (d == 0)
            {
                return 0;
            }
            if (d == 1)
            {
                return 1;
            }

            string strD = d.ToString();
            int D = strD.Length;
            int n = 0;
            float x;
            if ((D % 2) == 0)
            {
                n = (D - 2) / 2;
                x = 6.0f * Pow(10f, n);
            }
            else
            {
                n = (D - 1) / 2;
                x = 2.0f * Pow(10f, n);
            }
            int precision = (strD.Length > 6 ? strD.Length : 6);

            return x;
        }

        protected override void BeforeRun()
        {
            Console.WriteLine("SSchocke Playground");
        }

        protected override void Run()
        {
            //RunSqrtTest();
            //FloatingPointTest();
            MapTest();
        }

        private string FtoA(float val)
        {
            if (val == 0.0f)
            {
                return "0.0";
            }

            byte[] singleBytes = BitConverter.GetBytes(val);
            Int32 hexVal = BitConverter.ToInt32(singleBytes, 0);
            Int32 mantissa, intPart = 0, fracPart = 0;
            Int32 exp2;

            exp2 = ((hexVal >> 23) & 0xFF) -127;
            mantissa = (hexVal & 0xFFFFFF) | 0x800000;

            if (exp2 >= 31)
            {
                return "Single Overrange";
            }
            else if (exp2 < -23)
            {
                return "Single Underrange";
            }
            else if (exp2 >= 23)
            {
                intPart = mantissa << (exp2 - 23);
            }
            else if (exp2 >= 0)
            {
                intPart = mantissa >> (23 - exp2);
                fracPart = (mantissa << (exp2 + 1)) & 0xFFFFFF;
            }
            else
            {
                fracPart = (mantissa & 0xFFFFFF) >> (-(exp2 + 1));
            }

            string result = "";
            if (hexVal < 0)
            {
                result += "-";
            }
            result += ((UInt32)intPart).ToString();
            result += ".";
            if (fracPart == 0)
            {
                result += "0";
                return result;
            }

            for (int m = 0; m < 7; m++)
            {
                fracPart = (fracPart << 3) + (fracPart << 1);
                char p = (char)((fracPart >> 24) + '0');
                result += p;

                fracPart &= 0xFFFFFF;
            }
            while (result[result.Length - 1] == '0')
            {
                result = result.Substring(0, result.Length - 1);
            }

            return result;
        }
        private string DtoA(double val)
        {
            if (val == 0.0)
            {
                return "0.0";
            }

            byte[] doubleBytes = BitConverter.GetBytes(val);
            Int64 hexVal = BitConverter.ToInt64(doubleBytes, 0);
            Int64 mantissa, intPart = 0, fracPart = 0;
            Int32 exp2;

            exp2 = (int)((hexVal >> 52) & 0x07FF) - 1023;
            mantissa = (hexVal & 0xFFFFFFFFFFFFF) | 0x8000000000000;

            if (exp2 >= 63)
            {
                return "Double Overrange";
            }
            else if (exp2 < -52)
            {
                return "Double Underrange";
            }
            else if (exp2 >= 52)
            {
                intPart = mantissa << (exp2 - 52);
            }
            else if (exp2 >= 0)
            {
                intPart = mantissa >> (52 - exp2);
                fracPart = (mantissa << (exp2 + 1)) & 0x0FFFFFFFFFFFFF;
            }
            else
            {
                fracPart = (mantissa & 0x0FFFFFFFFFFFFF) >> (-(exp2 + 1));
            }

            string result = "";
            if (hexVal < 0)
            {
                result += "-";
            }
            result += ((UInt64)intPart).ToString();
            result += ".";
            if (fracPart == 0)
            {
                result += "0";
                return result;
            }

            for (int m = 0; m < 7; m++)
            {
                fracPart = (fracPart << 3) + (fracPart << 1);
                char p = (char)(((fracPart >> 53) & 0xFF) + '0');
                result += p;

                fracPart &= 0x0FFFFFFFFFFFFF;
            }
            while (result[result.Length - 1] == '0')
            {
                result = result.Substring(0, result.Length - 1);
            }

            return result;
        }

        private void MapTest()
        {
            Console.WriteLine("Map<int,string> test");
            var testMap = new IntMap<string>();
            Console.WriteLine("Map Item Count = " + testMap.Count);
            testMap.Add(1, "Hello");
            Console.WriteLine("Map Item Count = " + testMap.Count);
            testMap.Add(2, "Hello Again");
            Console.WriteLine("Map Item Count = " + testMap.Count);
            testMap.Add(3, "Goodbye");
            Console.WriteLine("Map Item Count = " + testMap.Count);
            for (int j = 4; j < 20; j++)
            {
                testMap.Add(j, "Test " + j.ToString());
            }
            Console.WriteLine("Map Item Count = " + testMap.Count);

            int[] keys = testMap.Keys;
            for (int i = 0; i < keys.Length; i++)
            {
                Console.WriteLine(testMap[keys[i]]);
            }

            testMap.Remove(10);
            testMap.Remove(19);
            Console.WriteLine("Map Item Count = " + testMap.Count);

            keys = testMap.Keys;
            for (int i = 0; i < keys.Length; i++)
            {
                Console.WriteLine(testMap[keys[i]]);
            }

            Console.WriteLine("Map<string,float> test");
            var testMap2 = new StringMap<float>();
            Console.WriteLine("Map Item Count = " + testMap2.Count);
            testMap2.Add("a", 0.1f);
            testMap2.Add("b", 0.2f);
            testMap2.Add("c", 0.3f);
            Console.WriteLine("Map Item Count = " + testMap2.Count);

            Console.WriteLine("a=" + testMap2["a"].ToString() + ",b=" + testMap2["b"].ToString() + ",c=" + testMap2["c"].ToString());
            Stop();
        }

        private void FloatingPointTest()
        {
            float flt;
            double dbl;

            Console.WriteLine("Float Test Values");
            flt = 1234.1234f;
            Console.WriteLine("1234.1234 = " + FtoA(flt));
            flt = 100.0f;
            Console.WriteLine("100.0 = " + FtoA(flt));
            flt = 0.0067f;
            Console.WriteLine("0.0067 = " + FtoA(flt));
            flt = 1897654321.123456f;
            Console.WriteLine("1897654321.123456 = " + FtoA(flt));
            flt = -223344.332211f;
            Console.WriteLine("-223344.332211 = " + FtoA(flt));

            Console.WriteLine("Double Test Values");
            dbl = 1234.1234;
            Console.WriteLine("1234.1234 = " + DtoA(dbl));
            dbl = 100.0;
            Console.WriteLine("100.0 = " + DtoA(dbl));
            dbl = 0.0067;
            Console.WriteLine("0.0067 = " + DtoA(dbl));
            dbl = 1897654321.123456;
            Console.WriteLine("1897654321.123456 = " + DtoA(dbl));
            dbl = -223344.332211;
            Console.WriteLine("-223344.332211 = " + DtoA(dbl));
            //Console.WriteLine(dbl.ToString());

            Stop();
        }

        private void RunSqrtTest()
        {
            Console.WriteLine("Calculating Square Root of 125348");
            float x = Sqrt(125348.0f);
            Console.WriteLine("x = " + x.ToString());
            Stop();
        }

        private void InitDictionaryTest()
        {
            //testDictComparer = new DictIntComparer();
            //testDict = new Dictionary<int, string>(testDictComparer);
        }
        private void RunDictionaryTest()
        {
            //Console.WriteLine("Dictionary Count: " + testDict.Count);
            //Console.WriteLine("Add 1");
            //testDict.Add(1, "Hello");
            //Console.WriteLine("Add 2");
            //testDict.Add(2, "Hello Again");
            //Console.WriteLine("Add 3");
            //testDict.Add(3, "Goodbye");
            //Console.WriteLine("Add Done");

            //testDictComparer.Equals(1, 2);
            //testDictComparer.GetHashCode(3);
        }
    }
}
