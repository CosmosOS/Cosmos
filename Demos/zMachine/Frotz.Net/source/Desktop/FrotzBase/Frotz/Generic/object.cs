/* object.c - Object manipulation opcodes
 *	Copyright (c) 1995-1997 Stefan Jokisch
 *
 * This file is part of Frotz.
 *
 * Frotz is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * Frotz is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA
 */

using zword = System.UInt16;
using zbyte = System.Byte;

using Frotz;
using Frotz.Constants;

namespace Frotz.Generic {
    internal static class CObject {
        internal static zword MAX_OBJECT = 2000;

        internal static zword O1_PARENT = 4;
        internal static zword O1_SIBLING = 5;
        internal static zword O1_CHILD = 6;
        internal static zword O1_PROPERTY_OFFSET = 7;
        internal static zword O1_SIZE = 9;

        internal static zword O4_PARENT = 6;
        internal static zword O4_SIBLING = 8;
        internal static zword O4_CHILD = 10;
        internal static zword O4_PROPERTY_OFFSET = 12;
        internal static zword O4_SIZE = 14;

        /*
         * object_address
         *
         * Calculate the address of an object.
         *
         */

        internal static zword object_address(zword obj) {
            /* Check object number */

            if (obj > ((main.h_version <= ZMachine.V3) ? 255 : MAX_OBJECT)) {
                Text.print_string("@Attempt to address illegal object ");
                Text.print_num(obj);
                Text.print_string(".  This is normally fatal.");
                Buffer.new_line();
                Err.runtime_error(ErrorCodes.ERR_ILL_OBJ);
            }

            /* Return object address */

            if (main.h_version <= ZMachine.V3)
                return (zword)(main.h_objects + ((obj - 1) * O1_SIZE + 62));
            else
                return (zword)(main.h_objects + ((obj - 1) * O4_SIZE + 126));

        }/* object_address */

        /*
         * object_name
         *
         * Return the address of the given object's name.
         *
         */

        internal static zword object_name(zword object_var) {
            zword obj_addr;
            zword name_addr;

            obj_addr = object_address(object_var);

            /* The object name address is found at the start of the properties */

            if (main.h_version <= ZMachine.V3)
                obj_addr += O1_PROPERTY_OFFSET;
            else
                obj_addr += O4_PROPERTY_OFFSET;

            FastMem.LOW_WORD(obj_addr, out name_addr);

            return name_addr;

        }/* object_name */

        /*
         * first_property
         *
         * Calculate the start address of the property list associated with
         * an object.
         *
         */

        static zword first_property(zword obj) {
            zword prop_addr;
            zbyte size;

            /* Fetch address of object name */

            prop_addr = object_name(obj);

            /* Get length of object name */

            FastMem.LOW_BYTE(prop_addr, out size);

            /* Add name length to pointer */

            return (zword)(prop_addr + 1 + 2 * size);

        }/* first_property */

        /*
         * next_property
         *
         * Calculate the address of the next property in a property list.
         *
         */

        static zword next_property(zword prop_addr) {
            zbyte value;

            /* Load the current property id */

            FastMem.LOW_BYTE(prop_addr, out value);
            prop_addr++;

            /* Calculate the length of this property */

            if (main.h_version <= ZMachine.V3)
                value >>= 5;
            else if (!((value & 0x80) > 0))
                value >>= 6;
            else {

                FastMem.LOW_BYTE(prop_addr, out value);
                value &= 0x3f;

                if (value == 0) value = 64;	/* demanded by Spec 1.0 */

            }

            /* Add property length to current property pointer */

            return (zword)(prop_addr + value + 1);

        }/* next_property */

        /*
         * unlink_object
         *
         * Unlink an object from its parent and siblings.
         *
         */

        static void unlink_object(zword object_var) {
            zword obj_addr;
            zword parent_addr;
            zword sibling_addr;

            if (object_var == 0) {
                Err.runtime_error(ErrorCodes.ERR_REMOVE_OBJECT_0);
                return;
            }

            obj_addr = object_address(object_var);

            if (main.h_version <= ZMachine.V3) {

                zbyte parent;
                zbyte younger_sibling;
                zbyte older_sibling;
                zbyte zero = 0;

                /* Get parent of object, and return if no parent */

                obj_addr += O1_PARENT;
                FastMem.LOW_BYTE(obj_addr, out parent);
                if (parent == 0)
                    return;

                /* Get (older) sibling of object and set both parent and sibling
                   pointers to 0 */

                FastMem.SET_BYTE(obj_addr, zero);
                obj_addr += (zword)(O1_SIBLING - O1_PARENT);
                FastMem.LOW_BYTE(obj_addr, out older_sibling);
                FastMem.SET_BYTE(obj_addr, zero);

                /* Get first child of parent (the youngest sibling of the object) */

                parent_addr = (zword)(object_address(parent) + O1_CHILD);
                FastMem.LOW_BYTE(parent_addr, out younger_sibling);

                /* Remove object from the list of siblings */

                if (younger_sibling == object_var)
                    FastMem.SET_BYTE(parent_addr, older_sibling);
                else {
                    do {
                        sibling_addr = (zword)(object_address(younger_sibling) + O1_SIBLING);
                        FastMem.LOW_BYTE(sibling_addr, out younger_sibling);
                    } while (younger_sibling != object_var);
                    FastMem.SET_BYTE(sibling_addr, older_sibling);
                }

            } else {

                zword parent;
                zword younger_sibling;
                zword older_sibling;
                zword zero = 0;

                /* Get parent of object, and return if no parent */

                obj_addr += O4_PARENT;
                FastMem.LOW_WORD(obj_addr, out parent);
                if (parent == 0)
                    return;

                /* Get (older) sibling of object and set both parent and sibling
                   pointers to 0 */

                FastMem.SET_WORD(obj_addr, zero);
                obj_addr += (zword)(O4_SIBLING - O4_PARENT);
                FastMem.LOW_WORD(obj_addr, out older_sibling);
                FastMem.SET_WORD(obj_addr, zero);

                /* Get first child of parent (the youngest sibling of the object) */

                parent_addr = (zword)(object_address(parent) + O4_CHILD);
                FastMem.LOW_WORD(parent_addr, out younger_sibling);

                /* Remove object from the list of siblings */

                if (younger_sibling == object_var)
                    FastMem.SET_WORD(parent_addr, older_sibling);
                else {
                    do {
                        sibling_addr = (zword)(object_address(younger_sibling) + O4_SIBLING);
                        FastMem.LOW_WORD(sibling_addr, out younger_sibling);
                    } while (younger_sibling != object_var);
                    FastMem.SET_WORD(sibling_addr, older_sibling);
                }

            }

        }/* unlink_object */

        /*
         * z_clear_attr, clear an object attribute.
         *
         *	Process.zargs[0] = object
         *	Process.zargs[1] = number of attribute to be cleared
         *
         */

        internal static void z_clear_attr() {
            zword obj_addr;
            zbyte value;

            if (main.story_id == Story.SHERLOCK)
                if (Process.zargs[1] == 48)
                    return;

            if (Process.zargs[1] > ((main.h_version <= ZMachine.V3) ? 31 : 47))
                Err.runtime_error(ErrorCodes.ERR_ILL_ATTR);

            /* If we are monitoring attribute assignment display a short note */

            if (main.option_attribute_assignment == true) {
                Stream.stream_mssg_on ();
                Text.print_string ("@clear_attr ");
                Text.print_object (Process.zargs[0]);
                Text.print_string (" ");
                Text.print_num (Process.zargs[1]);
                Stream.stream_mssg_off ();
            }

            if (Process.zargs[0] == 0) {
                Err.runtime_error(ErrorCodes.ERR_CLEAR_ATTR_0);
                return;
            }

            /* Get attribute address */

            obj_addr = (zword)(object_address(Process.zargs[0]) + Process.zargs[1] / 8);

            /* Clear attribute bit */

            FastMem.LOW_BYTE(obj_addr, out value);
            value &= (zbyte)(~(0x80 >> (Process.zargs[1] & 7)));
            FastMem.SET_BYTE(obj_addr, value);

        }/* z_clear_attr */

        /*
         * z_jin, branch if the first object is inside the second.
         *
         *	Process.zargs[0] = first object
         *	Process.zargs[1] = second object
         *
         */

        internal static void z_jin() {
            zword obj_addr;

            /* If we are monitoring object locating display a short note */

            if (main.option_object_locating == true) {
                Stream.stream_mssg_on ();
                Text.print_string ("@jin ");
                Text.print_object(Process.zargs[0]);
                Text.print_string(" ");
                Text.print_object(Process.zargs[1]);
                Stream.stream_mssg_off();
            }

            if (Process.zargs[0] == 0) {
                Err.runtime_error(ErrorCodes.ERR_JIN_0);
                Process.branch(0 == Process.zargs[1]);
                return;
            }

            obj_addr = object_address(Process.zargs[0]);

            if (main.h_version <= ZMachine.V3) {

                zbyte parent;

                /* Get parent id from object */

                obj_addr += O1_PARENT;
                FastMem.LOW_BYTE(obj_addr, out parent);

                /* Branch if the parent is obj2 */

                Process.branch(parent == Process.zargs[1]);

            } else {

                zword parent;

                /* Get parent id from object */

                obj_addr += O4_PARENT;
                FastMem.LOW_WORD(obj_addr, out parent);

                /* Branch if the parent is obj2 */

                Process.branch(parent == Process.zargs[1]);

            }

        }/* z_jin */

        /*
         * z_get_child, store the child of an object.
         *
         *	Process.zargs[0] = object
         *
         */

        internal static void z_get_child() {
            zword obj_addr;

            /* If we are monitoring object locating display a short note */

            if (main.option_object_locating == true) {
                Stream.stream_mssg_on();
                Text.print_string("@get_child ");
                Text.print_object(Process.zargs[0]);
                Stream.stream_mssg_off();
            }

            if (Process.zargs[0] == 0) {
                Err.runtime_error(ErrorCodes.ERR_GET_CHILD_0);
                Process.store(0);
                Process.branch(false);
                return;
            }

            obj_addr = object_address(Process.zargs[0]);

            if (main.h_version <= ZMachine.V3) {

                zbyte child;

                /* Get child id from object */

                obj_addr += O1_CHILD;
                FastMem.LOW_BYTE(obj_addr, out child);

                /* Store child id and branch */

                Process.store(child);
                Process.branch(child > 0);

            } else {

                zword child;

                /* Get child id from object */

                obj_addr += O4_CHILD;
                FastMem.LOW_WORD(obj_addr, out child);

                /* Store child id and branch */

                Process.store(child);
                Process.branch(child > 0);

            }

        }/* z_get_child */

        /*
         * z_get_next_prop, store the number of the first or next property.
         *
         *	Process.zargs[0] = object
         *	Process.zargs[1] = address of current property (0 gets the first property)
         *
         */

        internal static void z_get_next_prop() {
            zword prop_addr;
            zbyte value;
            zbyte mask;

            if (Process.zargs[0] == 0) {
                Err.runtime_error(ErrorCodes.ERR_GET_NEXT_PROP_0);
                Process.store(0);
                return;
            }

            /* Property id is in bottom five (six) bits */

            mask = (zbyte)((main.h_version <= ZMachine.V3) ? 0x1f : 0x3f);

            /* Load address of first property */

            prop_addr = first_property(Process.zargs[0]);

            if (Process.zargs[1] != 0) {

                /* Scan down the property list */

                do {
                    FastMem.LOW_BYTE(prop_addr, out value);
                    prop_addr = next_property(prop_addr);
                } while ((value & mask) > Process.zargs[1]);

                /* Exit if the property does not exist */

                if ((value & mask) != Process.zargs[1])
                    Err.runtime_error(ErrorCodes.ERR_NO_PROP);

            }

            /* Return the property id */

            FastMem.LOW_BYTE(prop_addr, out value);
            Process.store((zword)(value & mask));

        }/* z_get_next_prop */

        /*
         * z_get_parent, store the parent of an object.
         *
         *	Process.zargs[0] = object
         *
         */

        internal static void z_get_parent() {
            zword obj_addr;

            /* If we are monitoring object locating display a short note */

            if (main.option_object_locating == true) {
                Stream.stream_mssg_on();
                Text.print_string ("@get_parent ");
                Text.print_object(Process.zargs[0]);
                Stream.stream_mssg_off();
            }

            if (Process.zargs[0] == 0) {
                Err.runtime_error(ErrorCodes.ERR_GET_PARENT_0);
                Process.store(0);
                return;
            }

            obj_addr = object_address(Process.zargs[0]);

            if (main.h_version <= ZMachine.V3) {

                zbyte parent;

                /* Get parent id from object */

                obj_addr += O1_PARENT;
                FastMem.LOW_BYTE(obj_addr, out parent);

                /* Store parent */

                Process.store(parent);

            } else {

                zword parent;

                /* Get parent id from object */

                obj_addr += O4_PARENT;
                FastMem.LOW_WORD(obj_addr, out parent);

                /* Store parent */

                Process.store(parent);

            }

        }/* z_get_parent */

        /*
         * z_get_prop, store the value of an object property.
         *
         *	Process.zargs[0] = object
         *	Process.zargs[1] = number of property to be examined
         *
         */

        internal static void z_get_prop() {
            zword prop_addr;
            zword wprop_val;
            zbyte bprop_val;
            zbyte value;
            zbyte mask;

            if (Process.zargs[0] == 0) {
                Err.runtime_error(ErrorCodes.ERR_GET_PROP_0);
                Process.store(0);
                return;
            }

            /* Property id is in bottom five (six) bits */

            mask = (zbyte)((main.h_version <= ZMachine.V3) ? 0x1f : 0x3f);

            /* Load address of first property */

            prop_addr = first_property(Process.zargs[0]);

            /* Scan down the property list */

            for (; ; ) {
                FastMem.LOW_BYTE(prop_addr, out value);
                if ((value & mask) <= Process.zargs[1])
                    break;
                prop_addr = next_property(prop_addr);
            }

            if ((value & mask) == Process.zargs[1]) {	/* property found */

                /* Load property (byte or word sized) */

                prop_addr++;

                if ((main.h_version <= ZMachine.V3 && !((value & 0xe0) > 0)) || (main.h_version >= ZMachine.V4 && !((value & 0xc0) > 0))) {

                    FastMem.LOW_BYTE(prop_addr, out bprop_val);
                    wprop_val = bprop_val;

                } else FastMem.LOW_WORD(prop_addr, out wprop_val);

            } else {	/* property not found */

                /* Load default value */

                prop_addr = (zword)(main.h_objects + 2 * (Process.zargs[1] - 1));
                FastMem.LOW_WORD(prop_addr, out wprop_val);

            }

            /* Store the property value */

            Process.store(wprop_val);

        }/* z_get_prop */

        /*
         * z_get_prop_addr, store the address of an object property.
         *
         *	Process.zargs[0] = object
         *	Process.zargs[1] = number of property to be examined
         *
         */

        internal static void z_get_prop_addr() {
            zword prop_addr;
            zbyte value;
            zbyte mask;

            if (Process.zargs[0] == 0) {
                Err.runtime_error(ErrorCodes.ERR_GET_PROP_ADDR_0);
                Process.store(0);
                return;
            }

            if (main.story_id == Story.BEYOND_ZORK)
                if (Process.zargs[0] > MAX_OBJECT) { Process.store(0); return; }

            /* Property id is in bottom five (six) bits */

            mask = (zbyte)((main.h_version <= ZMachine.V3) ? 0x1f : 0x3f);

            /* Load address of first property */

            prop_addr = first_property(Process.zargs[0]);

            /* Scan down the property list */

            for (; ; ) {
                FastMem.LOW_BYTE(prop_addr, out value);
                if ((value & mask) <= Process.zargs[1])
                    break;
                prop_addr = next_property(prop_addr);
            }

            /* Calculate the property address or return zero */

            if ((value & mask) == Process.zargs[1]) {

                if (main.h_version >= ZMachine.V4 && (value & 0x80) > 0)
                    prop_addr++;
                Process.store((zword)(prop_addr + 1));

            } else Process.store(0);

        }/* z_get_prop_addr */

        /*
         * z_get_prop_len, store the length of an object property.
         *
         * 	Process.zargs[0] = address of property to be examined
         *
         */

        internal static void z_get_prop_len() {
            zword addr;
            zbyte value;

            /* Back up the property pointer to the property id */

            addr = (zword)(Process.zargs[0] - 1);
            FastMem.LOW_BYTE(addr, out value);

            /* Calculate length of property */

            if (main.h_version <= ZMachine.V3)
                value = (zbyte)((value >> 5) + 1);
            else if (!((value & 0x80) > 0))
                value = (zbyte)((value >> 6) + 1);
            else {

                value &= 0x3f;

                if (value == 0) value = 64;	/* demanded by Spec 1.0 */

            }

            /* Store length of property */

            Process.store(value);

        }/* z_get_prop_len */

        /*
         * z_get_sibling, store the sibling of an object.
         *
         *	Process.zargs[0] = object
         *
         */

        internal static void z_get_sibling() {
            zword obj_addr;

            if (Process.zargs[0] == 0) {
                Err.runtime_error(ErrorCodes.ERR_GET_SIBLING_0);
                Process.store(0);
                Process.branch(false);
                return;
            }

            obj_addr = object_address(Process.zargs[0]);

            if (main.h_version <= ZMachine.V3) {

                zbyte sibling;

                /* Get sibling id from object */

                obj_addr += O1_SIBLING;
                FastMem.LOW_BYTE(obj_addr, out sibling);

                /* Store sibling and branch */

                Process.store(sibling);
                Process.branch(sibling > 0); // TODO I'm not sure about this logic Process.branch (sibling);
                // I think it means if the sibling isn't zero, jump..

            } else {

                zword sibling;

                /* Get sibling id from object */

                obj_addr += O4_SIBLING;
                FastMem.LOW_WORD(obj_addr, out sibling);

                /* Store sibling and branch */

                Process.store(sibling);
                Process.branch(sibling > 0);

            }

        }/* z_get_sibling */

        /*
         * z_insert_obj, make an object the first child of another object.
         *
         *	Process.zargs[0] = object to be moved
         *	Process.zargs[1] = destination object
         *
         */

        internal static void z_insert_obj() {
            zword obj1 = Process.zargs[0];
            zword obj2 = Process.zargs[1];
            zword obj1_addr;
            zword obj2_addr;

            /* If we are monitoring object movements display a short note */

            if (main.option_object_movement == true) {
                Stream.stream_mssg_on();
                Text.print_string ("@move_obj ");
                Text.print_object (obj1);
                Text.print_string (" ");
                Text.print_object (obj2);
                Stream.stream_mssg_off();
            }

            if (obj1 == 0) {
                Err.runtime_error(ErrorCodes.ERR_MOVE_OBJECT_0);
                return;
            }

            if (obj2 == 0) {
                Err.runtime_error(ErrorCodes.ERR_MOVE_OBJECT_TO_0);
                return;
            }

            /* Get addresses of both objects */

            obj1_addr = object_address(obj1);
            obj2_addr = object_address(obj2);

            /* Remove object 1 from current parent */

            unlink_object(obj1);

            /* Make object 1 first child of object 2 */

            if (main.h_version <= ZMachine.V3) {

                zbyte child;

                obj1_addr += O1_PARENT;
                FastMem.SET_BYTE(obj1_addr, (zbyte)obj2);
                obj2_addr += O1_CHILD;
                FastMem.LOW_BYTE(obj2_addr, out child);
                FastMem.SET_BYTE(obj2_addr, (zbyte)obj1);
                obj1_addr += (zword)(O1_SIBLING - O1_PARENT);
                FastMem.SET_BYTE(obj1_addr, child);

            } else {

                zword child;

                obj1_addr += O4_PARENT;
                FastMem.SET_WORD(obj1_addr, obj2);
                obj2_addr += O4_CHILD;
                FastMem.LOW_WORD(obj2_addr, out child);
                FastMem.SET_WORD(obj2_addr, obj1);
                obj1_addr += (zword)(O4_SIBLING - O4_PARENT);
                FastMem.SET_WORD(obj1_addr, child);

            }

        }/* z_insert_obj */

        /*
         * z_put_prop, set the value of an object property.
         *
         *	Process.zargs[0] = object
         *	Process.zargs[1] = number of property to set
         *	Process.zargs[2] = value to set property to
         *
         */

        internal static void z_put_prop() {
            zword prop_addr;
            zbyte value;
            zbyte mask;

            if (Process.zargs[0] == 0) {
                Err.runtime_error(ErrorCodes.ERR_PUT_PROP_0);
                return;
            }

            /* Property id is in bottom five or six bits */

            mask = (zbyte)((main.h_version <= ZMachine.V3) ? 0x1f : 0x3f);

            /* Load address of first property */

            prop_addr = first_property(Process.zargs[0]);

            /* Scan down the property list */

            for (; ; ) {
                FastMem.LOW_BYTE(prop_addr, out value);
                if ((value & mask) <= Process.zargs[1])
                    break;
                prop_addr = next_property(prop_addr);
            }

            /* Exit if the property does not exist */

            if ((value & mask) != Process.zargs[1])
                Err.runtime_error(ErrorCodes.ERR_NO_PROP);

            /* Store the new property value (byte or word sized) */

            prop_addr++;

            if ((main.h_version <= ZMachine.V3 && !((value & 0xe0) > 0)) || (main.h_version >= ZMachine.V4 && !((value & 0xc0) > 0))) {
                zbyte v = (zbyte)Process.zargs[2];
                FastMem.SET_BYTE(prop_addr, v);
            } else {
                zword v = Process.zargs[2];
                FastMem.SET_WORD(prop_addr, v);
            }

        }/* z_put_prop */

        /*
         * z_remove_obj, unlink an object from its parent and siblings.
         *
         *	Process.zargs[0] = object
         *
         */

        internal static void z_remove_obj() {

            /* If we are monitoring object movements display a short note */

            if (main.option_object_movement == true) {
                Stream.stream_mssg_on();
                Text.print_string("@remove_obj ");
                Text.print_object(Process.zargs[0]);
                Stream.stream_mssg_off();
            }

            /* Call unlink_object to do the job */

            unlink_object(Process.zargs[0]);

        }/* z_remove_obj */

        /*
         * z_set_attr, set an object attribute.
         *
         *	Process.zargs[0] = object
         *	Process.zargs[1] = number of attribute to set
         *
         */

        internal static void z_set_attr() {
            zword obj_addr;
            zbyte value;

            if (main.story_id == Story.SHERLOCK)
                if (Process.zargs[1] == 48)
                    return;

            if (Process.zargs[1] > ((main.h_version <= ZMachine.V3) ? 31 : 47))
                Err.runtime_error(ErrorCodes.ERR_ILL_ATTR);

            /* If we are monitoring attribute assignment display a short note */

            if (main.option_attribute_assignment == true) {
                Stream.stream_mssg_on();
                Text.print_string ("@set_attr ");
                Text.print_object (Process.zargs[0]);
                Text.print_string (" ");
                Text.print_num (Process.zargs[1]);
                Stream.stream_mssg_off();
            }

            if (Process.zargs[0] == 0) {
                Err.runtime_error(ErrorCodes.ERR_SET_ATTR_0);
                return;
            }

            /* Get attribute address */

            obj_addr = (zword)(object_address(Process.zargs[0]) + Process.zargs[1] / 8);

            /* Load attribute byte */

            FastMem.LOW_BYTE(obj_addr, out value);

            /* Set attribute bit */

            value |= (zbyte)(0x80 >> (Process.zargs[1] & 7));

            /* Store attribute byte */

            FastMem.SET_BYTE(obj_addr, value);

        }/* z_set_attr */

        /*
         * z_test_attr, branch if an object attribute is set.
         *
         *	Process.zargs[0] = object
         *	Process.zargs[1] = number of attribute to test
         *
         */

        internal static void z_test_attr() {
            zword obj_addr;
            zbyte value;

            if (Process.zargs[1] > ((main.h_version <= ZMachine.V3) ? 31 : 47))
                Err.runtime_error(ErrorCodes.ERR_ILL_ATTR);

            /* If we are monitoring attribute testing display a short note */

            if (main.option_attribute_testing == true) {
                Stream.stream_mssg_on();
                Text.print_string ("@test_attr ");
                Text.print_object(Process.zargs[0]);
                Text.print_string(" ");
                Text.print_num(Process.zargs[1]);
                Stream.stream_mssg_off();
            }

            if (Process.zargs[0] == 0) {
                Err.runtime_error(ErrorCodes.ERR_TEST_ATTR_0);
                Process.branch(false);
                return;
            }

            /* Get attribute address */

            obj_addr = (zword)(object_address(Process.zargs[0]) + Process.zargs[1] / 8);

            /* Load attribute byte */

            FastMem.LOW_BYTE(obj_addr, out value);

            /* Test attribute */

            Process.branch((value & (0x80 >> (Process.zargs[1] & 7))) > 0);
        }/* z_test_attr */
    }
}