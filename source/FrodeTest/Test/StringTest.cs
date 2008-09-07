using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrodeTest.Test
{
    public class StringTest
    {
        public static void RunTest()
        {
            Check.SetHeadingText("Testing System.String - Static methods");

            //Static methods
            //Check.Text = "String.Compare";
            //Check.Validate(String.Compare("test", "test") == 0);
            //Check.Validate(String.Compare("test", "other") == 1);

            Check.Text = "String.CompareOrdinal";
            Check.Validate(String.CompareOrdinal("test", "test") == 0);
            Check.Validate(String.CompareOrdinal("test", "other") == 5);

            Check.Text = "String.Concat";
            Check.Validate(String.Concat("A", "B", "C", "D").Equals("ABCD"));

            Check.Text = "String.Copy";
            Check.Validate(String.Copy("test").Equals("test"));

            Check.Text = "String.Equals";
            Check.Validate(String.Equals("test", "test", StringComparison.OrdinalIgnoreCase));
            Check.Validate(!String.Equals("test", "other", StringComparison.CurrentCulture));

            Check.Text = "String.Format";
            Check.Validate(String.Format("Word:{0}", "Format").Equals("Word:Format"));

            //Check.Text = "String.Intern";
            //Check.Validate(String.Intern("test").Equals("test"));

            //Check.Text = "String.IsInterned";
            //Check.Validate(String.IsInterned("test").Equals("test"));

            Check.Text = "String.IsNullOrEmpty";
            Check.Validate(String.IsNullOrEmpty(null));
            Check.Validate(String.IsNullOrEmpty(String.Empty));
            Check.Validate(!String.IsNullOrEmpty("not empty"));

            Check.Text = "String.Join";
            Check.Validate(String.Join(":", new string[] { "Hello", "World" }, 0, 2).Equals("Hello:World"));


            Check.SetHeadingText("Testing System.String - Instance methods");

            Check.Text = "String.Clone";
            Check.Validate("test".Clone().ToString() == "test");

            Check.Text = "String.CompareTo";
            Check.Validate("test".CompareTo("test") == 0);

            Check.Text = "String.Contains";
            Check.Validate("test".Contains("e"));

            Check.Text = "String.CopyTo";
            char[] destination = new char[10];
            "test".CopyTo(0, destination, 0, 4);
            Check.Validate((destination[0] == 't') && (destination[1] == 'e') && (destination[2] == 's') && (destination[3] == 't'));

            Check.Text = "String.EndsWith";
            Check.Validate("test".EndsWith("st"));
            Check.Validate("test".EndsWith("st", StringComparison.CurrentCulture));
            Check.Validate("test".EndsWith("st", true, null));
            Check.Validate("test".EndsWith("test"));
            Check.Validate(!"test".EndsWith("VeryLongText"));

            Check.Text = "String.IndexOf";
            Check.Validate("test".IndexOf('t', 0, 2) == 0);
            Check.Validate("test".IndexOf('B', 1, 1) == -1);
            Check.Validate("test".IndexOf("st") == 2);
            Check.Validate("test".IndexOf("es", 1, 3, StringComparison.CurrentCulture) == 1);

            Check.Text = "String.IndexOfAny";
            Check.Validate("test".IndexOfAny(new char[] { 'a', 'b' }, 0, 4) == -1);
            Check.Validate("test".IndexOfAny(new char[] { 'e', 's' }, 0, 4) == 1);

            Check.Text = "String.Insert";
            Check.Validate("Hello".Insert(5, " World").Equals("Hello World"));
            
            //Check.Text = "String.IsNormalized";
            //Check.Validate("test".IsNormalized());

            Check.Text = "String.LastIndexOf";
            Check.Validate("Readme.txt".LastIndexOf('.') == 6);

            Console.WriteLine("Press any key to continue");
            Console.ReadLine();

            Check.Text = "String.LastIndexOfAny";
            Check.Validate("test".LastIndexOfAny(new char[] { 'a', 'b' }, 0, 4) == -1);
            Check.Validate("test".LastIndexOfAny(new char[] { 'a', 't' }, 3, 4) == 3);

            Check.Text = "String.Length";
            Check.Validate("StringWithLength18".Length == 18);

            //Check.Text = "String.Normalize";
            //Check.Validate("test".Normalize(NormalizationForm.FormKD).Equals("test"));

            Check.Text = "String.PadLeft";
            Check.Validate("test".PadLeft(8, '!').Equals("!!!!test"));

            Check.Text = "String.PadRight";
            Check.Validate("test".PadRight(8, '!').Equals("test!!!!"));

            Check.Text = "String.Remove";
            Check.Validate("test".Remove(2).Equals("te"));
            Check.Validate("test".Remove(1, 2).Equals("tt"));


            Check.Text = "String.Replace(char, char)";
            Check.Validate("test".Replace('t', 'p').Equals("pesp"));
            Check.Text = "String.Replace(string, string)";
            Check.Validate("test".Replace("es", "amti").Equals("tamtit")); //Uses .Insert - fix that first.

            Check.Text = "String.Split";
            Check.Validate("Hello World".Split(new string[] { "l" }, 30, StringSplitOptions.RemoveEmptyEntries).Length == 3);

            Check.Text = "String.StartsWith";
            Check.Validate("test".StartsWith("te"));
            Check.Validate("test".StartsWith("te", StringComparison.CurrentCulture));
            Check.Validate(!"test".StartsWith("boo", false, System.Globalization.CultureInfo.CurrentCulture));
            Check.Validate("test".StartsWith("test"));
            Check.Validate(!"test".StartsWith("testlong"));


            Check.Text = "String.Substring";
            Check.Validate("test".Substring(1, 2).Equals("es"));

            Check.Text = "String.ToCharArray";
            Check.Validate("test".ToCharArray().Length == 4);
            Check.Validate(("test".ToCharArray()[0] == 't') && ("test".ToCharArray()[2] == 's'));

            Check.Text = "String.ToLower";
            Check.Validate("TEST!".ToLower().Equals("test!"));
            Check.Validate("wOrDs".ToLower(null).Equals("words"));

            Check.Text = "String.ToLowerInvariant";
            Check.Validate("tESt".ToLowerInvariant().Equals("test"));

            Check.Text = "String.ToString";
            Check.Validate("test".ToString().Equals("test"));

            Check.Text = "String.ToUpper";
            Check.Validate("test".ToUpper().Equals("TEST"));

            Check.Text = "String.ToUpperInvariant";
            Check.Validate("test".ToUpperInvariant().Equals("TEST"));

            Check.Text = "String.Trim";
            Check.Validate("  test  .".Trim(new char[] { ' ', '.' }).Equals("test"));

            Check.Text = "String.TrimEnd";
            Check.Validate("test".TrimEnd(new char[] {'t'}).Equals("tes"));

            Check.Text = "String.TrimStart";
            Check.Validate("test".TrimStart(new char[] { 't' }).Equals("est"));



            // VARIOUS BUGS ETC.

            //Add char and string
            //Bug discovered 7.june. SysFault when adding char and string.
            //string added = string.Empty;
            //added = ((char)('c')) + "oncatenating char and string works.";
            //Console.WriteLine(added);

            //StringBuilder sb = new StringBuilder();
            //sb.Append("String");
            //sb.Append("Builder");
            //sb.Append(Environment.NewLine);
            //sb.Append("Works");
            //Console.WriteLine(sb.ToString());

            //Bug, found 4.aug
            //Printing 0:\ will print previous string in buffer instead!
            //Console.WriteLine("This should be printed ONCE");
            //Console.WriteLine("\\");
            //Console.WriteLine(@"0:\");
            //Console.WriteLine(@"\");
            //Console.WriteLine("Long string \\ Long String");
            //Console.WriteLine("Back to normal");

            //Bug
            //string xTemp = "\\";
            //Console.WriteLine(xTemp.Length);
            //for (int i = 0; i < xTemp.Length; i++)
            //{
            //    Console.Write(((byte)xTemp[i]).ToString());
            //}

            // Add \t as Tab
            //Console.WriteLine("Column1\tColumn2");


        }
    }
}
