using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace CoreLib.Locking
{
    public static class Interlocked
    {

        // Methods
        private static extern void _CompareExchange(TypedReference location1, TypedReference value, object comparand);
        private static extern void _Exchange(TypedReference location1, TypedReference value);
        //public static int Add(ref int location1, int value);
        //public static long Add(ref long location1, long value);
        public static extern int CompareExchange(ref int location1, int value, int comparand);
      //  public static T CompareExchange<T>(ref T location1, T value, T comparand) where T : class;
        public static extern double CompareExchange(ref double location1, double value, double comparand);
        public static extern long CompareExchange(ref long location1, long value, long comparand);
        public static extern IntPtr CompareExchange(ref IntPtr location1, IntPtr value, IntPtr comparand);
        public static extern object CompareExchange(ref object location1, object value, object comparand);
        public static extern float CompareExchange(ref float location1, float value, float comparand);
        public static extern int Decrement(ref int location);
        public static extern long Decrement(ref long location);
        public static extern double Exchange(ref double location1, double value);
        public static extern int Exchange(ref int location1, int value);
        public static extern long Exchange(ref long location1, long value);
        public static extern IntPtr Exchange(ref IntPtr location1, IntPtr value);
        public static extern object Exchange(ref object location1, object value);
        public static extern float Exchange(ref float location1, float value);
     //   public static T Exchange<T>(ref T location1, T value) where T : class;
        internal static extern int ExchangeAdd(ref int location1, int value);
        internal static extern long ExchangeAdd(ref long location1, long value);
        public static extern int Increment(ref int location);
        public static extern long Increment(ref long location);

        public static extern void Increment(UIntPtr location);
        public static extern void Decrement(UIntPtr location);

   //     public static long Read(ref long location);


    }
}
