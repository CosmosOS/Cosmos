using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Sys = Cosmos.System;

namespace CryptoTest
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            char_map = new List<string>();
            foreach (char c in "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890-=[]\\';/.,<>?:\"{}!@#$%^&*()_+~`")
            {
                char_map.Add((char_map.Count + 1) + "|" + c);
            }
            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
        }

        protected override void Run()
        {
            Console.WriteLine("Please input a string:");
            string str = Console.ReadLine();
            string orig = str;
            int i = 0;
            while(i < 10)
            {
                str = double_str(str);
                i++;
            }
            Console.WriteLine("Done.\nOriginal: " + orig + "\nDoubled: " + str);
        }

        public string double_str(string val)
        {
            string res = "";
            foreach(char c in val)
            {
                res += repeat(c, 2);
            }
            return res;
        }

        public string repeat(char c, int times)
        {
            string res = "";
            while(res.Length != times)
            {
                res += c;
            }
            return res;
        }

        public string encrypt(string value)
        {
            string enc = "";
            foreach(char c in value)
            {
                enc += get_map_in(c) + ",";
            }
            return enc;
        }

        public List<string> char_map = null;

        public string get_map_in(char c)
        {
            foreach(string s in char_map)
            {
                string[] split = s.Split('|');
                if(split[1] == c.ToString())
                {
                    return split[0];
                }
            }
            return "";
        }

        public string get_map_out(string index)
        {
            foreach (string s in char_map)
            {
                string[] split = s.Split('|');
                if (split[0] == index)
                {
                    return split[1];
                }
            }
            return "";
        }


        public string decrypt(string value)
        {
            string dec = "";
            while(value.Length > 0)
            {
                string cVal = value.Substring(0, value.IndexOf(','));
                dec += get_map_out(cVal);
            }
            return dec;
        }
    }
}
