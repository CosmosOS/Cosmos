namespace DuNodes.HAL.FileSystem.Base
{
    public static class Util
    {
        public static string RemoveNonprintableChars(string str)
        {
            string str1 = "";
            foreach (char ch in str)
            {
                if ((int)(byte)ch >= 32 && (int)(byte)ch <= 126)
                    str1 += ch.ToString();
            }
            return str1;
        }

        public static string Remove(string str, char with)
        {
            string str1 = "";
            foreach (char ch in str)
            {
                if (ch.ToString() != with.ToString())
                    str1 += ch.ToString();
            }
            return str1;
        }

        public static bool isWhiteSpace(char c)
        {
            return (int)c == 32 || (int)c == 13 || (int)c == 10;
        }

        public static bool isLetter(char c)
        {
            byte num = (byte)c;
            return (int)num >= 65 && (int)num <= 90 || (int)num >= 97 && (int)num <= 122;
        }

        public static bool isLetterOrDigit(char c)
        {
            if (Util.isLetter(c))
                return true;
            byte num = (byte)c;
            return (int)num >= 48 && (int)num <= 58;
        }

        public static bool isDigit(char c)
        {
            byte num = (byte)c;
            return (int)num >= 48 && (int)num <= 58;
        }

        public static bool Contains(string Str, char c)
        {
            foreach (int num in Str)
            {
                if (num == (int)c)
                    return true;
            }
            return false;
        }

        public static int IndexOf(string str, char c)
        {
            int num1 = 0;
            foreach (int num2 in str)
            {
                if (num2 == (int)c)
                    return num1;
                ++num1;
            }
            return -1;
        }

        public static string cleanName(string name)
        {
            if (name.Substring(0, 1) == FileSystem.Root.Seperator.ToString())
                name = name.Substring(1, name.Length - 1);
            if (name.Substring(name.Length - 1, 1) == FileSystem.Root.Seperator.ToString())
                name = name.Substring(0, name.Length - 1);
            return name;
        }

        public static int LastIndexOf(string This, char ch)
        {
            int num1 = -1;
            int num2 = 0;
            foreach (int num3 in This)
            {
                if (num3 == (int)ch)
                    num1 = num2;
                ++num2;
            }
            return num1;
        }
    }
}
