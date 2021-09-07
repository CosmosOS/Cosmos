namespace ZLibrary.Machine
{
    public static class ZRandom
    {
        public static int a = 1;

        public static int interval;
        public static int counter;

        public static void Seed(int value)
        {
            if (value == 0)
            {
                a = 1;
                interval = 0;
            }
            else if (value < 1000)
            {
                counter = 0;
                interval = value;
            }
            else
            {
                a = value;
                interval = 0;
            }
        }
    }
}
