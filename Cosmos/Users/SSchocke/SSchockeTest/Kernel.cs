using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;
using Cosmos.Core;
using System.Net;
using Cosmos.Hardware;
using Cosmos.Hardware.Drivers.PCI.Network;
using Cosmos.System.Network;
using IPv4 = Cosmos.System.Network.IPv4;

namespace SSchockeTest
{
    public class Kernel : Sys.Kernel
    {
        public Kernel()
        {
            ClearScreen = false;
        }
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

        //public class IntMap<TValue> : Map<int, TValue>
        //{
        //    protected override bool keysEqual(int key1, int key2)
        //    {
        //        return (key1 == key2);
        //    }
        //}
        //public class StringMap<TValue> : Map<string, TValue>
        //{
        //    protected override bool keysEqual(string key1, string key2)
        //    {
        //        return (key1 == key2);
        //    }
        //}

        //public float Pow(float x, float y)
        //{
        //    if (y == 0)
        //    {
        //        return 1;
        //    }
        //    else if (y == 1)
        //    {
        //        return x;
        //    }
        //    else
        //    {
        //        float xResult = x;

        //        for (int i = 2; i <= y; i++)
        //        {
        //            xResult = xResult * x;
        //        }

        //        return xResult;
        //    }
        //}
        //private float Sqrt(float d)
        //{
        //    if ((d < 0) || (float.IsNaN(d) == true) || (float.IsNegativeInfinity(d) == true))
        //    {
        //        return float.NaN;
        //    }
        //    if (float.IsPositiveInfinity(d) == true)
        //    {
        //        return float.PositiveInfinity;
        //    }
        //    if (d == 0)
        //    {
        //        return 0;
        //    }
        //    if (d == 1)
        //    {
        //        return 1;
        //    }

        //    string strD = d.ToString();
        //    int D = strD.Length;
        //    int n = 0;
        //    float x;
        //    if ((D % 2) == 0)
        //    {
        //        n = (D - 2) / 2;
        //        x = 6.0f * Pow(10f, n);
        //    }
        //    else
        //    {
        //        n = (D - 1) / 2;
        //        x = 2.0f * Pow(10f, n);
        //    }
        //    int precision = (strD.Length > 6 ? strD.Length : 6);

        //    return x;
        //}

        //internal class BaseClass
        //{
        //    public int baseField;

        //    internal BaseClass()
        //    {
        //        initFields();
        //    }

        //    protected virtual void initFields()
        //    {
        //        baseField = 1;
        //    }
        //}
        //internal class SubClass : BaseClass
        //{
        //    public int subField;

        //    internal SubClass()
        //        : base()
        //    {
        //    }

        //    protected override void initFields()
        //    {
        //        base.initFields();
        //        subField = 2;
        //    }
        //}
        //internal class SubSubClass : SubClass
        //{
        //    public int subsubField;

        //    internal SubSubClass()
        //        :base()
        //    {
        //    }

        //    protected override void initFields()
        //    {
        //        base.initFields();
        //        subsubField = 3;
        //    }
        //}

        protected override void BeforeRun()
        {
            Console.WriteLine("SSchocke Test Playground");

            Console.WriteLine("Finding network devices...");
            AMDPCNetII.FindAll();

            NetworkDevice nic = NetworkDevice.Devices[0];

            IPv4.Address myIP = new IPv4.Address(192, 168, 1, 51);
            IPv4.Address mySubnet = new IPv4.Address(255, 255, 255, 0);
            IPv4.Address myGateway = new IPv4.Address(192, 168, 1, 1);
            IPv4.Config myConfig = new IPv4.Config(myIP, mySubnet, myGateway);

            NetworkStack.ConfigIP(nic, myConfig);
            nic.Enable();

            Console.WriteLine("Init Done... Starting main loop");
        }

        protected override void Run()
        {
            //SubSubClass instance = new SubSubClass();

            //Console.WriteLine("BaseClass.field = " + instance.baseField);
            //Console.WriteLine("SubClass.field = " + instance.subField);
            //Console.WriteLine("SubSubClass.field = " + instance.subsubField);

            //Stop();

            //RunSqrtTest();
            //FloatingPointTest();
            //MapTest();
            //UInt64Test(10, 20);
            //DoubleTest(10.0, 20.0);
            //TestDoubleSupport();
            //while (true)
            //{
            //    NetworkStack.Update();
            //}

            //Stop();
        }

        private static void TestDoubleSupport()
        {
            double val = Double.PositiveInfinity;
            if (Double.IsPositiveInfinity(val) == true)
            {
                Console.WriteLine("Double.IsPositiveInfinity() returns true");
            }
            else
            {
                Console.WriteLine("Double.IsPositiveInfinity() returns false");
            }
            if (Double.IsInfinity(val) == true)
            {
                Console.WriteLine("Double.IsInfinity() returns true");
            }
            else
            {
                Console.WriteLine("Double.IsInfinity() returns false");
            }
            val = Double.NegativeInfinity;
            if (Double.IsNegativeInfinity(val) == true)
            {
                Console.WriteLine("Double.IsNegativeInfinity() returns true");
            }
            else
            {
                Console.WriteLine("Double.IsNegativeInfinity() returns false");
            }
            if (Double.IsInfinity(val) == true)
            {
                Console.WriteLine("Double.IsInfinity() returns true");
            }
            else
            {
                Console.WriteLine("Double.IsInfinity() returns false");
            }
        }

        //private void UInt64Test(UInt64 paramVal1, UInt64 paramVal2)
        //{
        //    UInt64 val1, val2, val3, val4, val5;

        //    Console.WriteLine("UInt64 testing");
        //    val1 = 1;
        //    val2 = 2;
        //    val3 = 3;
        //    val4 = 4;
        //    val5 = paramVal1 * paramVal2;

        //    Console.WriteLine("paramVal1 = " + paramVal1.ToString());
        //    Console.WriteLine("paramVal2 = " + paramVal2.ToString());
        //    Console.WriteLine("val1 = " + val1.ToString());
        //    Console.WriteLine("val2 = " + val2.ToString());
        //    Console.WriteLine("val3 = " + val3.ToString());
        //    Console.WriteLine("val4 = " + val4.ToString());
        //    Console.WriteLine("val5 = " + val5.ToString());

        //    Stop();
        //}

        //private void DoubleTest(Double paramVal1, Double paramVal2)
        //{
        //    Double val1, val2, val3, val4, val5;

        //    Console.WriteLine("Double testing");
        //    val1 = 1;
        //    val2 = 2;
        //    val3 = 3;
        //    val4 = 4;
        //    val5 = paramVal1 * paramVal2;

        //    Console.WriteLine("paramVal1 = " + paramVal1.ToString());
        //    Console.WriteLine("paramVal2 = " + paramVal2.ToString());
        //    Console.WriteLine("val1 = " + val1.ToString());
        //    Console.WriteLine("val2 = " + val2.ToString());
        //    Console.WriteLine("val3 = " + val3.ToString());
        //    Console.WriteLine("val4 = " + val4.ToString());
        //    Console.WriteLine("val5 = " + val5.ToString());

        //    Stop();
        //}

        //private string FtoA(float val)
        //{
        //    if (val == 0.0f) return "0.0";
        //    if (Single.IsNaN(val) == true) return "NaN";
        //    if (Single.IsPositiveInfinity(val) == true) return "Infinity";
        //    if (Single.IsNegativeInfinity(val) == true) return "-Infinity";

        //    byte[] singleBytes = BitConverter.GetBytes(val);
        //    Int32 hexVal = BitConverter.ToInt32(singleBytes, 0);
        //    Int32 mantissa, intPart = 0, fracPart = 0;
        //    Int32 exp2;

        //    exp2 = ((hexVal >> 23) & 0xFF) - 127;
        //    mantissa = (hexVal & 0xFFFFFF) | 0x800000;

        //    if (exp2 >= 31)
        //    {
        //        return "Single Overrange";
        //    }
        //    else if (exp2 < -23)
        //    {
        //        return "Single Underrange";
        //    }
        //    else if (exp2 >= 23)
        //    {
        //        intPart = mantissa << (exp2 - 23);
        //    }
        //    else if (exp2 >= 0)
        //    {
        //        intPart = mantissa >> (23 - exp2);
        //        fracPart = (mantissa << (exp2 + 1)) & 0xFFFFFF;
        //    }
        //    else
        //    {
        //        fracPart = (mantissa & 0xFFFFFF) >> (-(exp2 + 1));
        //    }

        //    string result = "";
        //    if (hexVal < 0)
        //    {
        //        result += "-";
        //    }
        //    result += ((UInt32)intPart).ToString();
        //    int used_digits = ((UInt32)intPart).ToString().Length;
        //    result += ".";
        //    if (fracPart == 0)
        //    {
        //        result += "0";
        //        return result;
        //    }

        //    if (used_digits >= 7)
        //    {
        //        used_digits = 6;
        //    }
        //    for (int m = used_digits; m < 7; m++)
        //    {
        //        fracPart = (fracPart << 3) + (fracPart << 1);
        //        char p = (char)((fracPart >> 24) + '0');
        //        result += p;

        //        fracPart &= 0xFFFFFF;
        //    }
        //    fracPart = (fracPart << 3) + (fracPart << 1);
        //    char remain = (char)((fracPart >> 24) + '0');
        //    if ((remain > '5') && (result[result.Length - 1] > '0'))
        //    {
        //        char[] answer = result.ToCharArray();
        //        int digitPos = answer.Length - 1;
        //        char digit = result[digitPos];
        //        answer[digitPos] = (char)(digit + 1);
        //        while (answer[digitPos] > '9')
        //        {
        //            answer[digitPos] = '0';
        //            digitPos--;
        //            digit = result[digitPos];
        //            if (digit == '.')
        //            {
        //                digitPos--;
        //                digit = result[digitPos];
        //            }
        //            answer[digitPos] = (char)(digit + 1);
        //        }

        //        result = new String(answer);
        //    }

        //    while (result[result.Length - 1] == '0')
        //    {
        //        result = result.Substring(0, result.Length - 1);
        //    }

        //    return result;
        //}
        //private string DtoA(double val)
        //{
        //    if (val == 0.0f) return "0.0";
        //    if (Double.IsNaN(val) == true) return "NaN";
        //    if (Double.IsPositiveInfinity(val) == true) return "Infinity";
        //    if (Double.IsNegativeInfinity(val) == true) return "-Infinity";

        //    byte[] doubleBytes = BitConverter.GetBytes(val);
        //    Int64 hexVal = BitConverter.ToInt64(doubleBytes, 0);
        //    Int64 mantissa, intPart = 0, fracPart = 0;
        //    Int32 exp2;

        //    exp2 = (int)((hexVal >> 52) & 0x07FF) - 1023;
        //    mantissa = (hexVal & 0x1FFFFFFFFFFFFF) | 0x10000000000000;

        //    if (exp2 >= 63)
        //    {
        //        return "Double Overrange";
        //    }
        //    else if (exp2 < -52)
        //    {
        //        return "Double Underrange";
        //    }
        //    else if (exp2 >= 52)
        //    {
        //        intPart = mantissa << (exp2 - 52);
        //    }
        //    else if (exp2 >= 0)
        //    {
        //        intPart = mantissa >> (52 - exp2);
        //        fracPart = (mantissa << (exp2 + 1)) & 0x1FFFFFFFFFFFFF;
        //    }
        //    else
        //    {
        //        fracPart = (mantissa & 0x1FFFFFFFFFFFFF) >> (-(exp2 + 1));
        //    }

        //    string result = "";
        //    if (hexVal < 0)
        //    {
        //        result += "-";
        //    }
        //    result += ((UInt64)intPart).ToString();
        //    int used_digits = ((UInt64)intPart).ToString().Length;
        //    result += ".";
        //    if (fracPart == 0)
        //    {
        //        result += "0";
        //        return result;
        //    }

        //    if (used_digits >= 15)
        //    {
        //        used_digits = 14;
        //    }
        //    for (int m = used_digits; m < 15; m++)
        //    {
        //        fracPart = (fracPart << 3) + (fracPart << 1);
        //        char p = (char)(((fracPart >> 53) & 0xFF) + '0');
        //        result += p;

        //        fracPart &= 0x1FFFFFFFFFFFFF;
        //    }
        //    fracPart = (fracPart << 3) + (fracPart << 1);
        //    char remain = (char)((fracPart >> 53) + '0');
        //    if ((remain > '5') && (result[result.Length - 1] > '0'))
        //    {
        //        char[] answer = result.ToCharArray();
        //        int digitPos = answer.Length - 1;
        //        char digit = result[digitPos];
        //        answer[digitPos] = (char)(digit + 1);
        //        while (answer[digitPos] > '9')
        //        {
        //            answer[digitPos] = '0';
        //            digitPos--;
        //            digit = result[digitPos];
        //            if (digit == '.')
        //            {
        //                digitPos--;
        //                digit = result[digitPos];
        //            }
        //            answer[digitPos] = (char)(digit + 1);
        //        }

        //        result = new String(answer);
        //    }

        //    while (result[result.Length - 1] == '0')
        //    {
        //        result = result.Substring(0, result.Length - 1);
        //    }

        //    return result;
        //}

        //private void MapTest()
        //{
        //    Console.WriteLine("Map<int,string> test");
        //    var testMap = new IntMap<string>();
        //    Console.WriteLine("Map Item Count = " + testMap.Count);
        //    testMap.Add(1, "Hello");
        //    Console.WriteLine("Map Item Count = " + testMap.Count);
        //    testMap.Add(2, "Hello Again");
        //    Console.WriteLine("Map Item Count = " + testMap.Count);
        //    testMap.Add(3, "Goodbye");
        //    Console.WriteLine("Map Item Count = " + testMap.Count);
        //    for (int j = 4; j < 20; j++)
        //    {
        //        testMap.Add(j, "Test " + j.ToString());
        //    }
        //    Console.WriteLine("Map Item Count = " + testMap.Count);

        //    int[] keys = testMap.Keys;
        //    for (int i = 0; i < keys.Length; i++)
        //    {
        //        Console.WriteLine(testMap[keys[i]]);
        //    }

        //    Console.ReadLine();

        //    testMap.Remove(10);
        //    testMap.Remove(19);
        //    Console.WriteLine("Map Item Count = " + testMap.Count);

        //    keys = testMap.Keys;
        //    for (int i = 0; i < keys.Length; i++)
        //    {
        //        Console.WriteLine(testMap[keys[i]]);
        //    }

        //    Console.WriteLine("Map<string,float> test");
        //    var testMap2 = new StringMap<float>();
        //    Console.WriteLine("Map Item Count = " + testMap2.Count);
        //    testMap2.Add("a", 0.1f);
        //    testMap2.Add("b", 0.2f);
        //    testMap2.Add("c", 0.3f);
        //    Console.WriteLine("Map Item Count = " + testMap2.Count);

        //    Console.WriteLine("a=" + testMap2["a"].ToString() + ",b=" + testMap2["b"].ToString() + ",c=" + testMap2["c"].ToString());
        //    Stop();
        //}

        //private void FloatingPointTest()
        //{
        //    float flt;
        //    double dbl;

        //    Console.WriteLine("Float Test Values");
        //    //flt = 0.0f;
        //    //for (int i = 0; i < 10; i++)
        //    //{
        //    //    flt = i / 10.0f;
        //    //    Console.WriteLine((i / 10).ToString() + "." + (i % 10).ToString() + " = " + FtoA(flt) + ", " + flt.ToString());
        //    //    if ((i % 10) == 9)
        //    //    {
        //    //        Console.WriteLine();
        //    //        //Console.ReadLine();
        //    //    }
        //    //}
        //    //flt = 1234.1234f;
        //    //Console.WriteLine("1234.1234 = " + FtoA(flt) + ", " + flt.ToString());
        //    //flt = 100.0f;
        //    //Console.WriteLine("100.0 = " + FtoA(flt) + ", " + flt.ToString());
        //    //flt = 0.0067f;
        //    //Console.WriteLine("0.0067 = " + FtoA(flt) + ", " + flt.ToString());
        //    //flt = 1897654321.123456f;
        //    //Console.WriteLine("1897654321.123456 = " + FtoA(flt) + ", " + flt.ToString());
        //    //flt = -223344.332211f;
        //    //Console.WriteLine("-223344.332211 = " + FtoA(flt) + ", " + flt.ToString());
        //    flt = Single.NaN;
        //    Console.WriteLine("Single.NaN = " + FtoA(flt) + ", " + flt.ToString());
        //    flt = Single.PositiveInfinity;
        //    Console.WriteLine("Single.PositiveInfinity = " + FtoA(flt) + ", " + flt.ToString());
        //    flt = Single.NegativeInfinity;
        //    Console.WriteLine("Single.NegativeInfinity = " + FtoA(flt) + ", " + flt.ToString());

        //    Console.ReadLine();
        //    Console.Clear();

        //    Console.WriteLine("Double Test Values");
        //    //for (int i = 0; i < 10; i++)
        //    //{
        //    //    dbl = i / 10.0;
        //    //    Console.WriteLine((i / 10).ToString() + "." + (i % 10).ToString() + " = " + DtoA(dbl) + ", " + dbl.ToString());
        //    //    if ((i % 10) == 9)
        //    //    {
        //    //        Console.WriteLine();
        //    //        //Console.ReadLine();
        //    //    }
        //    //}
        //    //dbl = 0.1;
        //    //Console.WriteLine("0.1 = " + DtoA(dbl) + ", " + dbl.ToString());
        //    //dbl = 0.6;
        //    //Console.WriteLine("0.6 = " + DtoA(dbl) + ", " + dbl.ToString());
        //    //dbl = 1.0;
        //    //Console.WriteLine("1.0 = " + DtoA(dbl) + ", " + dbl.ToString());
        //    //dbl = 1234.1234;
        //    //Console.WriteLine("1234.1234 = " + DtoA(dbl) + ", " + dbl.ToString());
        //    //dbl = 100.0;
        //    //Console.WriteLine("100.0 = " + DtoA(dbl) + ", " + dbl.ToString());
        //    //dbl = 0.0067;
        //    //Console.WriteLine("0.0067 = " + DtoA(dbl) + ", " + dbl.ToString());
        //    //dbl = 1897654321.123456;
        //    //Console.WriteLine("1897654321.123456 = " + DtoA(dbl) + ", " + dbl.ToString());
        //    //dbl = -223344.332211;
        //    //Console.WriteLine("-223344.332211 = " + DtoA(dbl) + ", " + dbl.ToString());
        //    dbl = Double.NaN;
        //    Console.WriteLine("Double.NaN = " + DtoA(dbl) + ", " + dbl.ToString());
        //    dbl = Double.PositiveInfinity;
        //    Console.WriteLine("Double.PositiveInfinity = " + DtoA(dbl) + ", " + dbl.ToString());
        //    dbl = Double.NegativeInfinity;
        //    Console.WriteLine("Double.NegativeInfinity = " + DtoA(dbl) + ", " + dbl.ToString());

        //    Stop();
        //}

        //private void RunSqrtTest()
        //{
        //    Console.WriteLine("Calculating Square Root of 125348");
        //    float x = Sqrt(125348.0f);
        //    Console.WriteLine("x = " + x.ToString());
        //    Stop();
        //}

        //private void InitDictionaryTest()
        //{
        //    //testDictComparer = new DictIntComparer();
        //    //testDict = new Dictionary<int, string>(testDictComparer);
        //}
        //private void RunDictionaryTest()
        //{
        //    //Console.WriteLine("Dictionary Count: " + testDict.Count);
        //    //Console.WriteLine("Add 1");
        //    //testDict.Add(1, "Hello");
        //    //Console.WriteLine("Add 2");
        //    //testDict.Add(2, "Hello Again");
        //    //Console.WriteLine("Add 3");
        //    //testDict.Add(3, "Goodbye");
        //    //Console.WriteLine("Add Done");

        //    //testDictComparer.Equals(1, 2);
        //    //testDictComparer.GetHashCode(3);
        //}

        ////protected override void BeforeRun()
        ////{
        ////    Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
        ////}

        ////protected override void Run()
        ////{
        ////    Console.Write("Input: ");
        ////    var input = Console.ReadLine();
        ////    Console.Write("Text typed: ");
        ////    Console.WriteLine(input);
        ////}
    }
}
