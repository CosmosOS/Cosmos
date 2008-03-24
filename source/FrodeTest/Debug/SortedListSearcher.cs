using System;
using System.Collections.Generic;
using System.Text;

namespace FrodeTest.Debug
{
    class SortedListSearcher
    {
        /* Task from Matthijs:
         * well, now i'm working on the mapping list (sortedlist) for the class i showed you chad..
         * [18:12:04] Matthijs ter Woord sier : 
         * frode, find the fastest way to get the largest key (uint) in a SortedList smaller than a given value..
         * yes, a SortedList<uint, object>
         */

        public static void RunTest()
        {
            SortedList<UInt32, object> list = new SortedList<UInt32,object>();

            list.Add(1, "en");
            list.Add(2, "to");
            list.Add(3, "tre");
            list.Add(5, "fem");

            Console.WriteLine("Found value: " + SortedListSearcher.FindClosestValue(2, list));
            Console.WriteLine("Found value: " + SortedListSearcher.FindClosestValue(4, list));
        }

        public static object FindClosestValue(UInt32 value, SortedList<UInt32, object> list)
        {
            object foundValue;

            //Sanity check
            if (list.Count == 0)
                throw new ArgumentException("No items in SortedList");

            if (list.TryGetValue(value, out foundValue))
            {
                return foundValue;
            }
            else
            {
                //Slow, steps through each one.
                int index = 0;
                while (list.Keys[index] < value)
                    index++;

                return list.Values[index - 1];
            }
        }


    }
}
