using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frotz.Constants {
    public static class ErrorCodes {

        ///* Error codes */
        public static short ERR_TEXT_BUF_OVF = 1;	/* Text buffer overflow */
        public static short ERR_STORE_RANGE = 2;	/* Store out of dynamic memory */
        public static short ERR_DIV_ZERO = 3;		/* Division by zero */
        public static short ERR_ILL_OBJ = 4;		/* Illegal object */
        public static short ERR_ILL_ATTR = 5;		/* Illegal attribute */
        public static short ERR_NO_PROP = 6;		/* No such property */
        public static short ERR_STK_OVF = 7;		/* Stack overflow */
        public static short ERR_ILL_CALL_ADDR = 8;	/* Call to illegal address */
        public static short ERR_CALL_NON_RTN = 9;	/* Call to non-routine */
        public static short ERR_STK_UNDF = 10;		/* Stack underflow */
        public static short ERR_ILL_OPCODE = 11;	/* Illegal opcode */
        public static short ERR_BAD_FRAME = 12;	/* Bad stack frame */
        public static short ERR_ILL_JUMP_ADDR = 13;	/* Jump to illegal address */
        public static short ERR_SAVE_IN_INTER = 14;	/* Can't save while in interrupt */
        public static short ERR_STR3_NESTING = 15;	/* Nesting stream #3 too deep */
        public static short ERR_ILL_WIN = 16;		/* Illegal window */
        public static short ERR_ILL_WIN_PROP = 17;	/* Illegal window property */
        public static short ERR_ILL_PRINT_ADDR = 18;	/* Print at illegal address */
        public static short ERR_DICT_LEN = 19;	/* Illegal dictionary word length */
        public static short ERR_MAX_FATAL = 19;

        ///* Less serious errors */
        public static short ERR_JIN_0 = 20;		/* @jin called with object 0 */
        public static short ERR_GET_CHILD_0 = 21;	/* @get_child called with object 0 */
        public static short ERR_GET_PARENT_0 = 22;	/* @get_parent called with object 0 */
        public static short ERR_GET_SIBLING_0 = 23;	/* @get_sibling called with object 0 */
        public static short ERR_GET_PROP_ADDR_0 = 24;	/* @get_prop_addr called with object 0 */
        public static short ERR_GET_PROP_0 = 25;	/* @get_prop called with object 0 */
        public static short ERR_PUT_PROP_0 = 26;	/* @put_prop called with object 0 */
        public static short ERR_CLEAR_ATTR_0 = 27;	/* @clear_attr called with object 0 */
        public static short ERR_SET_ATTR_0 = 28;	/* @set_attr called with object 0 */
        public static short ERR_TEST_ATTR_0 = 29;	/* @test_attr called with object 0 */
        public static short ERR_MOVE_OBJECT_0 = 30;	/* @move_object called moving object 0 */
        public static short ERR_MOVE_OBJECT_TO_0 = 31;	/* @move_object called moving into object 0 */
        public static short ERR_REMOVE_OBJECT_0 = 32;	/* @remove_object called with object 0 */
        public static short ERR_GET_NEXT_PROP_0 = 33;	/* @get_next_prop called with object 0 */
        public static int ERR_NUM_ERRORS = 33;

        ///* There are four error reporting modes: never report errors;
        //  report only the first time a given error type occurs; report
        //  every time an error occurs; or treat all errors as fatal
        //  errors, killing the interpreter. I strongly recommend
        //  "report once" as the default. But you can compile in a
        //  different default by changing the definition of
        //  ERR_DEFAULT_REPORT_MODE. In any case, the player can
        //  specify a report mode on the command line by typing "-Z 0"
        //  through "-Z 3". */

        public static short ERR_REPORT_NEVER = (0);
        public static short ERR_REPORT_ONCE = (1);
        public static short ERR_REPORT_ALWAYS = (2);
        public static short ERR_REPORT_FATAL = (3);

        public static short ERR_DEFAULT_REPORT_MODE = ERR_REPORT_ONCE;
    }
}
