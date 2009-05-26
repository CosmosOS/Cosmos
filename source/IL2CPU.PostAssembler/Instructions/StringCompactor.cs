using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2CPU.PostAssembler
{
    internal class StringCompactor
    {

        const string SEPERATOR = " ";
        const string LABEL_SEPERATOR = "__"; // OR ___

        public string[] StringToCompact(string input)
        {
            // strip spaces in between []
            StripSpacesBetweenBrackets(input);


            return Compact(input, SEPERATOR); 
        }

        private void StripSpacesBetweenBrackets(string input)
        {
            int left = input.IndexOf("[");
            while (left != -1)
            {
                int right = input.IndexOf("]");
                if (right == -1)
                    break; //Todo Warn

                string sub = input.Substring(left, right - left + 1);
                var replace = sub.Replace(" ", string.Empty);

                input.Replace(sub, replace);

                left = input.IndexOf("[", left + 1);


            }
        }


        public string[] LabelToCompact(string input)
        {


            return Compact(input, LABEL_SEPERATOR); 
        }



        internal string[] Compact(string input , string seperator)
        {


            var temp = input.Split(seperator.ToCharArray() ,StringSplitOptions.None);

           List<string> result = new List<string>(temp.Count());

            foreach ( var subString  in temp)
                result.Add(String.Intern(subString));

            return result.ToArray(); 
        }



        public string CompactToString(string[] param)
        {
            return String.Join(SEPERATOR, param); 
        }

        public string CompactToLabel(string[] param)
        {
            return String.Join(LABEL_SEPERATOR, param);
        }
    }
}
