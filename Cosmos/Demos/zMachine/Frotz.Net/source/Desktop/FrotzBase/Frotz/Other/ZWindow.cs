using zword = System.UInt16;
using zbyte = System.Byte;

using Frotz;
using Frotz.Constants;

namespace Frotz.Other
{
    public class ZWindow // TODO I'd like to make this private again
    { // Making this a class so pointers will work
        public zword y_pos;
        public zword x_pos;
        public zword y_size;
        public zword x_size;
        public zword y_cursor;
        public zword x_cursor;
        public zword left;
        public zword right;
        public zword nl_routine;
        public zword nl_countdown;
        public zword style;
        public zword colour;
        public zword font;
        public zword font_size;
        public zword attribute;
        public zword line_count;
        public zword true_fore;
        public zword true_back;
        public int index; // szurgot


        public zword this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return y_pos;
                    case 1: return x_pos;
                    case 2: return y_size;
                    case 3: return x_size;
                    case 4: return y_cursor;
                    case 5: return x_cursor;
                    case 6: return left;
                    case 7: return right;
                    case 8: return nl_routine;
                    case 9: return nl_countdown;
                    case 10: return style;
                    case 11: return colour;
                    case 12: return font;
                    case 13: return font_size;
                    case 14: return attribute;
                    case 15: return line_count;
                    case 16: return true_fore;
                    case 17: return true_back;
                }
                return 0;
            }

            set
            {
                switch (index)
                {
                    case 0: y_pos = value; break;
                    case 1: x_pos = value; break;
                    case 2: y_size = value; break;
                    case 3: x_size = value; break;
                    case 4: y_cursor = value; break;
                    case 5: x_cursor = value; break;
                    case 6: left = value; break;
                    case 7: right = value; break;
                    case 8: nl_routine = value; break;
                    case 9: nl_countdown = value; break;
                    case 10: style = value; break;
                    case 11: colour = value; break;
                    case 12: font = value; break;
                    case 13: font_size = value; break;
                    case 14: attribute = value; break;
                    case 15: line_count = value; break;
                    case 16: true_fore = value; break;
                    case 17: true_back = value; break;
                }
            }
        }

        public override string ToString()
        {
            return string.Format("Window: {0} Pos: {1}:{2} Size: {3}:{4} Cursor:{5}:{6}",
                index, x_pos, y_pos,
                x_size, y_size,
                x_cursor, y_cursor);
        }
    }
}