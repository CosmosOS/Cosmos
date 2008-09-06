using System;
using System.Collections.Generic;
using System.Text;

namespace FrodeTest.Test
{
    public class BoolTest
    {
        public static void RunTest()
        {
            //TESTING TRUE/FALSE TOSTRING
            bool yes = true;
            bool no = false;
            Console.WriteLine("true.ToString() gives: " + yes.ToString());
            Console.WriteLine("false.ToString() gives: " + no.ToString());

            //Testing returntype
            CompareNullReturnFalse();

            //Testing parsing
            Check.Text = "Bool Parse";
            if (Boolean.Parse("True"))
                Check.OK();
            else
                Check.Fail();

            //if (Boolean.Parse("tRUE")) //Doesn't work because of .Equals(string, StringComparison)
            //    Check.OK();
            //else
            //    Check.Fail();

            if (Boolean.Parse("False"))
                Check.Fail();
            else
                Check.OK();

            try
            {
                Check.Text = "ArgumentNull check";
                Boolean.Parse(null);
                Check.Fail();
            }
            catch (ArgumentNullException)
            {
                Check.OK();
            }
            catch (Exception ex)
            {
                Check.Fail();
            }

            Check.Text = "TryParse";
            bool result = false;
            Boolean.TryParse("True", out result);
            if (result)
                Check.OK();
            else
                Check.Fail();

            Check.Text = "TryParseShouldGiveFalse";
            if (!Boolean.TryParse("blabla", out result))
                Check.OK();
        }

        public static bool CompareNullReturnFalse()
        {
            return (GetNull() != null);
        }

        public static object GetNull()
        {
            return null;
        }
    }
}
