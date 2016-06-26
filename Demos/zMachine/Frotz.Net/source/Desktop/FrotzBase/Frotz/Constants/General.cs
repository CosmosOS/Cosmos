namespace Frotz.Constants {
// typedef unsigned char zbyte;
// typedef unsigned short zword;

    public static class General {

        /*** Constants that may be set at compile time ***/
        public static int TEXT_BUFFER_SIZE = 2000;
        public static int MAX_FILE_NAME = 256;
        public static int INPUT_BUFFER_SIZE = 200;
        public static int STACK_SIZE = 32768;

        public static int MAX_UNDO_SLOTS = 500;

        public static string DEFAULT_SAVE_NAME = "story.sav";
        public static string DEFAULT_SCRIPT_NAME = "story.scr";
        public static string DEFAULT_COMMAND_NAME = "story.rec";
        public static string DEFAULT_AUXILARY_NAME = "story.aux";

        public static string DEFAULT_SAVE_DIR = ".frotz-saves";
    }
}